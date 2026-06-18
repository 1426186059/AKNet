/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库
*        Author:许珂
*        StartTime:2024/11/01 00:00:00
*        ModifyTime:2026/2/1 20:26:47
*        Copyright:MIT软件许可证
************************************Copyright*****************************************/
using AKNet.Common;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AKNet.WebSocket.Server
{
    using WsWebSocket = System.Net.WebSockets.WebSocket;

    internal partial class ClientPeer
    {
        /// <summary>
        /// 同步完成 WebSocket HTTP Upgrade 握手（与 TCP 的 HandleConnectedSocket 对应）
        /// 握手完成后立即设置 CONNECTED 状态，启动后台接收循环
        /// </summary>
        public void PerformWebSocketHandshake(Socket socket)
        {
            try
            {
                mTcpClient = new System.Net.Sockets.TcpClient();
                mTcpClient.Client = socket;

                var stream = mTcpClient.GetStream();
                var cancellationToken = mCancellationTokenSource.Token;

                // 读取 HTTP 升级请求
                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead <= 0)
                {
                    DisConnectedWithNormal();
                    return;
                }

                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (!request.Contains("Upgrade: websocket", StringComparison.OrdinalIgnoreCase))
                {
                    DisConnectedWithNormal();
                    return;
                }

                // 提取 Sec-WebSocket-Key
                string key = ExtractWebSocketKey(request);
                if (string.IsNullOrEmpty(key))
                {
                    DisConnectedWithNormal();
                    return;
                }

                // 发送 HTTP 101 响应（同步）
                string acceptKey = WebSocketHelpers.ComputeAcceptKey(key);
                var response = $"HTTP/1.1 101 Switching Protocols\r\n" +
                               $"Upgrade: websocket\r\n" +
                               $"Connection: Upgrade\r\n" +
                               $"Sec-WebSocket-Accept: {acceptKey}\r\n\r\n";

                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                stream.Write(responseBytes, 0, responseBytes.Length);
                stream.Flush();

                // 创建 WebSocket 实例
                mWebSocket = WsWebSocket.CreateFromStream(stream, true, null, TimeSpan.FromSeconds(30));

                var remoteEp = socket.RemoteEndPoint as IPEndPoint;
                if (remoteEp != null)
                {
                    mIPEndPoint = new IPEndPoint(remoteEp.Address, remoteEp.Port);
                }

                MainThreadCheck.Check();
                SetSocketState(SOCKET_PEER_STATE.CONNECTED);

                // 启动后台接收循环
                _ = Task.Run(ReceiveLoopAsync);
            }
            catch (Exception e)
            {
                MainThreadCheck.Check();
                NetLog.LogWarning($"WebSocket 服务端 握手异常: {e.Message}");
                DisConnectedWithNormal();
            }
        }

        private static string ExtractWebSocketKey(string request)
        {
            foreach (var line in request.Split('\n'))
            {
                if (line.StartsWith("Sec-WebSocket-Key:", StringComparison.OrdinalIgnoreCase))
                {
                    return line.Substring("Sec-WebSocket-Key:".Length).Trim();
                }
            }
            return string.Empty;
        }

        private async Task ReceiveLoopAsync()
        {
            var receiveBuffer = new byte[1024 * 64];
            var cancellationToken = mCancellationTokenSource.Token;

            try
            {
                while (mWebSocket != null &&
                       (mWebSocket.State == WebSocketState.Open ||
                        mWebSocket.State == WebSocketState.CloseSent) &&
                       !cancellationToken.IsCancellationRequested)
                {
                    var result = await mWebSocket.ReceiveAsync(
                        new ArraySegment<byte>(receiveBuffer), cancellationToken)
                        .ConfigureAwait(false);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        if (mWebSocket.State == WebSocketState.Open ||
                            mWebSocket.State == WebSocketState.CloseReceived)
                        {
                            await mWebSocket.CloseOutputAsync(
                                WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None)
                                .ConfigureAwait(false);
                        }

                        MainThreadCheck.Check();
                        DisConnectedWithNormal();
                        break;
                    }

                    if (result.Count > 0)
                    {
                        lock (mReceiveStreamList)
                        {
                            mReceiveStreamList.WriteFrom(
                                new ReadOnlySpan<byte>(receiveBuffer, 0, result.Count));
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                MainThreadCheck.Check();
                NetLog.LogWarning($"WebSocket 服务端 接收异常: {e.Message}");
                DisConnectedWithNormal();
            }
            finally
            {
                bReceiveTaskRunning = false;
            }
        }

        private void TryFlushSendBuffer()
        {
            if (bSendTaskRunning)
                return;

            bSendTaskRunning = true;
            _ = Task.Run(SendLoopAsync);
        }

        private async Task SendLoopAsync()
        {
            try
            {
                while (true)
                {
                    int nLength;
                    lock (mSendStreamList)
                    {
                        nLength = mSendStreamList.Length;
                    }

                    if (nLength <= 0 || mWebSocket == null ||
                        (mWebSocket.State != WebSocketState.Open &&
                         mWebSocket.State != WebSocketState.CloseReceived))
                    {
                        break;
                    }

                    nLength = Math.Min(mSendBuffer.Length, nLength);
                    lock (mSendStreamList)
                    {
                        mSendStreamList.CopyTo(new Span<byte>(mSendBuffer, 0, nLength));
                    }

                    try
                    {
                        await mWebSocket.SendAsync(
                            new ArraySegment<byte>(mSendBuffer, 0, nLength),
                            WebSocketMessageType.Binary,
                            true,
                            mCancellationTokenSource.Token)
                            .ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        MainThreadCheck.Check();
                        NetLog.LogWarning($"WebSocket 服务端 发送异常: {e.Message}");
                        DisConnectedWithNormal();
                        break;
                    }

                    lock (mSendStreamList)
                    {
                        mSendStreamList.ClearBuffer(nLength);
                    }
                }
            }
            finally
            {
                bSendTaskRunning = false;

                bool hasData = false;
                lock (mSendStreamList)
                {
                    hasData = mSendStreamList.Length > 0;
                }

                if (hasData && mWebSocket != null &&
                    (mWebSocket.State == WebSocketState.Open ||
                     mWebSocket.State == WebSocketState.CloseReceived))
                {
                    bSendTaskRunning = true;
                    _ = Task.Run(SendLoopAsync);
                }
            }
        }

        private bool NetPackageExecute()
        {
            NetStreamReceivePackage mNetPackage = mServerMgr.mNetPackage;
            bool bSuccess = false;
            lock (mReceiveStreamList)
            {
                bSuccess = mServerMgr.mCryptoMgr.Decode(mReceiveStreamList, mNetPackage);
            }

            if (bSuccess)
            {
                if (CommonTcpLayerNetCommand.orInnerCommand(mNetPackage.nPackageId))
                {

                }
                else
                {
                    mServerMgr.mPackageManager.NetPackageExecute(this, mNetPackage);
                }
            }

            return bSuccess;
        }

        private void DisConnectedWithNormal()
        {
            SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
        }

        private void CloseWebSocket()
        {
            if (mWebSocket != null)
            {
                try
                {
                    if (mWebSocket.State == WebSocketState.Open ||
                        mWebSocket.State == WebSocketState.CloseReceived)
                    {
                        mWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None)
                            .ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                }
                catch { }
                finally
                {
                    mWebSocket.Dispose();
                    mWebSocket = null;
                }
            }

            if (mTcpClient != null)
            {
                try { mTcpClient.Close(); } catch { }
                mTcpClient = null;
            }
        }
    }
}
