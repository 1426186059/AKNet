// 客户端连接封装（分部类之二：发送相关，走 KNet 协议编码）。
using Fleck;
using KNet.Common;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;

namespace KNet.WebSocket.Server
{
    internal partial class ClientPeer
    {
        internal readonly object mWsLock = new object();
        internal IWebSocketConnection mWebSocket = null;
        internal System.Net.Sockets.TcpClient mTcpClient = null;

        public IPEndPoint GetIPEndPoint() 
        {
            if (mIPEndPoint == null)
            {
                mIPEndPoint =  new IPEndPoint(IPAddress.Parse(mWebSocket.ConnectionInfo.ClientIpAddress), mWebSocket.ConnectionInfo.ClientPort);
            }
            return mIPEndPoint;
        }

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
