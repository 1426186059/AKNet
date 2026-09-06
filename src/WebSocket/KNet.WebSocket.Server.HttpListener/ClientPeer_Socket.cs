/************************************Copyright*****************************************
 *  Project    : KNet
 *  Web        : https://github.com/1426186059/KNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/09/06 00:00:00
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using KNet.Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace KNet.WebSocket.Server
{
    using WsWebSocket = System.Net.WebSockets.WebSocket;

    internal partial class ClientPeer
    {
        internal readonly object mWsLock = new object();
        internal WsWebSocket mWebSocket = null;
        internal IPEndPoint mIPEndPoint = null;

        // 由 NetServerMain 在系统 HttpListener 完成 AcceptWebSocketAsync 之后调用：
        // 连接已建立，这里只登记 WebSocket 并启动收发循环（握手由系统 HttpListener 完成）。
        internal void AttachWebSocket(WsWebSocket ws, IPEndPoint endPoint)
        {
            lock (mWsLock) { mWebSocket = ws; }
            mIPEndPoint = endPoint;

            MainThreadCheck.Check();
            SetSocketState(SOCKET_PEER_STATE.CONNECTED);
            _ = Task.Run(ReceiveLoopAsync);
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
            }
        }
    }
}
