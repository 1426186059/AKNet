/************************************Copyright*****************************************
 *  Project    : AKNet
 *  Web        : https://github.com/825126369/AKNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/07/02 04:26:51
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using AKNet.Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace AKNet.WebSocket.Server
{
    using WsWebSocket = System.Net.WebSockets.WebSocket;

    internal partial class ClientPeer
    {
        internal readonly object mWsLock = new object();
        internal WsWebSocket mWebSocket = null;
        internal System.Net.Sockets.TcpClient mTcpClient = null;
        internal IPEndPoint mIPEndPoint = null;

        public void PerformWebSocketHandshake(Socket socket)
        {
            try
            {
                mTcpClient = new System.Net.Sockets.TcpClient();
                mTcpClient.Client = socket;
                var stream = mTcpClient.GetStream();

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead <= 0) { DisConnectedWithNormal(); return; }

                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (!request.Contains("Upgrade: websocket", StringComparison.OrdinalIgnoreCase))
                { DisConnectedWithNormal(); return; }

                string key = ExtractWebSocketKey(request);
                if (string.IsNullOrEmpty(key)) { DisConnectedWithNormal(); return; }

                string acceptKey = WebSocketHelpers.ComputeAcceptKey(key);
                byte[] responseBytes = Encoding.UTF8.GetBytes(
                    $"HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: {acceptKey}\r\n\r\n");
                stream.Write(responseBytes, 0, responseBytes.Length);
                stream.Flush();

                lock (mWsLock) { mWebSocket = WsWebSocket.CreateFromStream(stream, true, null, TimeSpan.FromSeconds(30)); }

                var remoteEp = socket.RemoteEndPoint as IPEndPoint;
                if (remoteEp != null) mIPEndPoint = new IPEndPoint(remoteEp.Address, remoteEp.Port);

                MainThreadCheck.Check();
                SetSocketState(SOCKET_PEER_STATE.CONNECTED);
                _ = Task.Run(ReceiveLoopAsync);
            }
            catch (Exception e)
            {
                MainThreadCheck.Check();
                NetLog.LogWarning($"握手异常: {e.Message}");
                DisConnectedWithNormal();
            }
        }

        public IPEndPoint GetIPEndPoint() { return mIPEndPoint; }

        private void SendNetStream(ReadOnlySpan<byte> mBufferSegment)
        {
            ResetSendHeartBeatTime();
            lock (mSendStreamList) 
            { 
                mSendStreamList.WriteFrom(mBufferSegment); 
            }

            if (!bSending)
            {
                bSending = true;
                _ = System.Threading.Tasks.Task.Run(SendLoopAsync);
            }
            else
            {
                if (!bSending && mSendStreamList.Length > 0)
                {
                    NetLog.LogError("SendNetStream Error");
                }
            }
        }

        private async System.Threading.Tasks.Task SendLoopAsync()
        {
            while (true)
            {
                WsWebSocket ws;
                lock (mWsLock) { ws = mWebSocket; }

                int nLength;
                lock (mSendStreamList) { nLength = mSendStreamList.Length; }
                if (nLength <= 0 || ws == null || ws.State != WebSocketState.Open)
                { bSending = false; return; }

                nLength = Math.Min(mSendBuffer.Length, nLength);
                lock (mSendStreamList) { mSendStreamList.CopyTo(new Span<byte>(mSendBuffer, 0, nLength)); }

                try
                {
                    await ws.SendAsync(new ArraySegment<byte>(mSendBuffer, 0, nLength),
                        WebSocketMessageType.Binary, true, System.Threading.CancellationToken.None)
                        .ConfigureAwait(false);

                    lock (mSendStreamList) { mSendStreamList.ClearBuffer(nLength); }
                }
                catch (InvalidOperationException) { bSending = false; return; }
                catch { bSending = false; DisConnectedWithNormal(); return; }
            }
        }

        private void DisConnectedWithNormal() { SetSocketState(SOCKET_PEER_STATE.DISCONNECTED); }

        internal void CloseWebSocket()
        {
            lock (mWsLock)
            {
                if (mWebSocket != null) { try { mWebSocket.Dispose(); } catch { } mWebSocket = null; }
                if (mTcpClient != null) { try { mTcpClient.Close(); } catch { } mTcpClient = null; }
            }
        }

        private static string ExtractWebSocketKey(string request)
        {
            foreach (var line in request.Split('\n'))
                if (line.StartsWith("Sec-WebSocket-Key:", StringComparison.OrdinalIgnoreCase))
                    return line.Substring("Sec-WebSocket-Key:".Length).Trim();
            return string.Empty;
        }
    }
}
