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
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace AKNet.WebSocket.Client
{
    // 非 WebGL 实现：使用 System.Net.WebSockets.ClientWebSocket
#if !UNITY_WEBGL || UNITY_EDITOR
    internal partial class NetClientMain
    {
        public void ReConnectServer()
        {
            bool Connected = mWebSocket != null &&
                             (mWebSocket.State == WebSocketState.Open ||
                              mWebSocket.State == WebSocketState.Connecting);

            if (Connected)
            {
                SetSocketState(SOCKET_PEER_STATE.CONNECTED);
            }
            else
            {
                ConnectServer(this.ServerIp, this.nServerPort);
            }
        }

        public void ConnectServer(string ServerAddr, int ServerPort)
        {
            Reset();
            this.ServerIp = ServerAddr;
            this.nServerPort = ServerPort;

            SetSocketState(SOCKET_PEER_STATE.CONNECTING);

            if (mIPEndPoint == null)
            {
                IPAddress mIPAddress = IPAddress.Parse(ServerAddr);
                mIPEndPoint = new IPEndPoint(mIPAddress, ServerPort);
            }

            CreateWebSocket();
            var uri = new Uri($"ws://{ServerAddr}:{ServerPort}/");
            NetLog.Log($"WebSocket 客户端 正在连接服务器: {uri}");

            Task.Run(async () =>
            {
                try
                {
                    await mWebSocket.ConnectAsync(uri, mCancellationTokenSource.Token).ConfigureAwait(false);
                    NetLog.Log($"WebSocket 客户端 连接服务器: {uri} 成功");

                    SetSocketState(SOCKET_PEER_STATE.CONNECTED);

                    if (!bReceiveTaskRunning)
                    {
                        bReceiveTaskRunning = true;
                        _ = Task.Run(ReceiveLoopAsync);
                    }
                }
                catch (Exception e)
                {
                    MainThreadCheck.Check();
                    NetLog.LogError($"WebSocket 客户端 连接服务器: {uri} 失败: {e.Message}");
                    DisConnectedWithError();
                }
            });
        }

        public bool DisConnectServer()
        {
            NetLog.Log("WebSocket 客户端 主动 断开服务器 Begin......");
            MainThreadCheck.Check();

            bool Connected = mWebSocket != null &&
                             (mWebSocket.State == WebSocketState.Open ||
                              mWebSocket.State == WebSocketState.CloseReceived);

            if (Connected)
            {
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTING);
                CloseSocket();
            }

            SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            NetLog.Log("WebSocket 客户端 主动 断开服务器 Finish......");
            return GetSocketState() == SOCKET_PEER_STATE.DISCONNECTED;
        }

        public IPEndPoint GetIPEndPoint()
        {
            return mIPEndPoint;
        }

        private void CreateWebSocket()
        {
            CloseSocket();
            mWebSocket = new ClientWebSocket();
        }

        private void CloseSocket()
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

            bReceiveTaskRunning = false;
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
                        MainThreadCheck.Check();
                        NetLog.Log("WebSocket 客户端 收到关闭消息");
                        DisConnectedWithError();
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
                NetLog.LogWarning($"WebSocket 客户端 接收异常: {e.Message}");
                DisConnectedWithError();
            }
            finally
            {
                bReceiveTaskRunning = false;
            }
        }

        private void PollWebSocketEvents() { }
    }
#endif

    // WebGL 实现：使用 JS 互操作
#if UNITY_WEBGL && !UNITY_EDITOR
    internal partial class NetClientMain
    {
        public void ReConnectServer()
        {
            if (mWebSocketInstanceId >= 0 && AKWebSocket_GetReadyState(mWebSocketInstanceId) == 1)
            {
                SetSocketState(SOCKET_PEER_STATE.CONNECTED);
            }
            else
            {
                ConnectServer(this.ServerIp, this.nServerPort);
            }
        }

        public void ConnectServer(string ServerAddr, int ServerPort)
        {
            Reset();
            this.ServerIp = ServerAddr;
            this.nServerPort = ServerPort;

            SetSocketState(SOCKET_PEER_STATE.CONNECTING);

            if (mIPEndPoint == null)
            {
                IPAddress mIPAddress = IPAddress.Parse(ServerAddr);
                mIPEndPoint = new IPEndPoint(mIPAddress, ServerPort);
            }

            mWebSocketInstanceId = ++sNextInstanceId;
            var uri = $"ws://{ServerAddr}:{ServerPort}/";
            NetLog.Log($"WebSocket 客户端 正在连接服务器: {uri}");

            int result = AKWebSocket_Connect(uri, mWebSocketInstanceId);
            if (result != 0)
            {
                SetSocketState(SOCKET_PEER_STATE.CONNECTED);
                NetLog.Log($"WebSocket 客户端 连接服务器: {uri} 成功");
            }
            else
            {
                NetLog.LogError($"WebSocket 客户端 连接服务器: {uri} 失败");
                DisConnectedWithError();
            }
        }

        public bool DisConnectServer()
        {
            NetLog.Log("WebSocket 客户端 主动 断开服务器 Begin......");
            MainThreadCheck.Check();

            if (mWebSocketInstanceId >= 0)
            {
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTING);
                CloseSocket();
            }

            SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            NetLog.Log("WebSocket 客户端 主动 断开服务器 Finish......");
            return GetSocketState() == SOCKET_PEER_STATE.DISCONNECTED;
        }

        public IPEndPoint GetIPEndPoint()
        {
            return mIPEndPoint;
        }

        private void CloseSocket()
        {
            if (mWebSocketInstanceId >= 0)
            {
                AKWebSocket_Close(mWebSocketInstanceId);
                mWebSocketInstanceId = -1;
            }
        }

        private void PollWebSocketEvents()
        {
            if (mWebSocketInstanceId < 0) return;

            // 轮询事件
            int evt = AKWebSocket_PollEvent(mWebSocketInstanceId);
            while (evt != 0)
            {
                switch (evt)
                {
                    case 2: // Closed
                        DisConnectedWithError();
                        break;
                    case 3: // Error
                        DisConnectedWithError();
                        break;
                }
                evt = AKWebSocket_PollEvent(mWebSocketInstanceId);
            }

            // 轮询消息
            int msgLen = AKWebSocket_PollMessageLength(mWebSocketInstanceId);
            while (msgLen > 0)
            {
                int copyLen = Math.Min(msgLen, mReceiveBuffer.Length);
                int actualLen = AKWebSocket_PollMessageCopy(mWebSocketInstanceId, mReceiveBuffer, copyLen);
                if (actualLen > 0)
                {
                    lock (mReceiveStreamList)
                    {
                        mReceiveStreamList.WriteFrom(new ReadOnlySpan<byte>(mReceiveBuffer, 0, actualLen));
                    }
                }
                msgLen = AKWebSocket_PollMessageLength(mWebSocketInstanceId);
            }
        }

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int AKWebSocket_Connect(string url, int instanceId);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void AKWebSocket_Close(int instanceId);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int AKWebSocket_Send(int instanceId, byte[] data, int dataLen);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int AKWebSocket_GetReadyState(int instanceId);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int AKWebSocket_PollEvent(int instanceId);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int AKWebSocket_PollMessageLength(int instanceId);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int AKWebSocket_PollMessageCopy(int instanceId, byte[] dest, int maxLen);
    }
#endif
}
