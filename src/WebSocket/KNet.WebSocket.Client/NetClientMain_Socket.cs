/************************************Copyright*****************************************
 *  Project    : KNet
 *  Web        : https://github.com/1426186059/KNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/07/28 00:39:11
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using KNet.Common;
using System;
using System.Diagnostics;
using System.Net;
#if !UNITY_WEBGL || UNITY_EDITOR
using System.Net.WebSockets;
using System.Threading.Tasks;
#endif

namespace KNet.WebSocket.Client
{
    internal partial class NetClientMain
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        public void ReConnectServer()
        {
            bool Connected = false;
            try { Connected = mWebSocket != null && mWebSocket.State == WebSocketState.Open; } catch { }
            if (Connected) SetSocketState(SOCKET_PEER_STATE.CONNECTED);
            else ConnectServer(this.ServerIp, this.nServerPort);
        }

        public void ConnectServer(string ServerAddr, int ServerPort)
        {
            MainThreadCheck.Check();
            Reset();
            this.ServerIp = ServerAddr;
            this.nServerPort = ServerPort;
            SetSocketState(SOCKET_PEER_STATE.CONNECTING);
            lock (mWsLock) { mWebSocket = new ClientWebSocket(); }

            if (mIPEndPoint == null)
            {
                IPAddress mIPAddress = IPAddress.Parse(ServerAddr);
                mIPEndPoint = new IPEndPoint(mIPAddress, ServerPort);
            }

            var uri = new Uri($"ws://{ServerAddr}:{ServerPort}/");
            NetLog.Log($"WebSocket 客户端 正在连接服务器: {uri}");

            Task.Run(async () =>
            {
                try
                {
                    ClientWebSocket ws;
                    lock (mWsLock) { ws = mWebSocket; }
                    if (ws == null) return;

                    await ws.ConnectAsync(uri, System.Threading.CancellationToken.None).ConfigureAwait(false);
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
                    NetLog.LogError($"WebSocket 客户端 连接服务器: {uri} 失败: {e.Message}");
                    DisConnectedWithError();
                }
            });
        }

        public bool DisConnectServer()
        {
            NetLog.Log("WebSocket 客户端 主动 断开服务器 Begin......");
            MainThreadCheck.Check();

            bool Connected = false;
            try { Connected = mWebSocket != null && mWebSocket.State == WebSocketState.Open; } catch { }

            if (Connected)
            {
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTING);
                lock (mWsLock) { CloseSocket(); }
            }
            else { SetSocketState(SOCKET_PEER_STATE.DISCONNECTED); }

            NetLog.Log("WebSocket 客户端 主动 断开服务器 Finish......");
            return GetSocketState() == SOCKET_PEER_STATE.DISCONNECTED;
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
                _ = Task.Run(SendLoopAsync);
            }
            else
            {
                if (!bSending && mSendStreamList.Length > 0)
                {
                    NetLog.LogError("SendNetStream Error");
                }
            }
        }

        private async Task SendLoopAsync()
        {
            while (true)
            {
                ClientWebSocket ws;
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
                catch { bSending = false; DisConnectedWithError(); return; }
            }
        }

        private void DisConnectedWithError()
        {
            if (GetSocketState() == SOCKET_PEER_STATE.DISCONNECTING)
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            else if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
            {
                if (mConfigInstance.bAutoReConnect)
                    SetSocketState(SOCKET_PEER_STATE.RECONNECTING);
                else
                    SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            }
        }
#endif
    }
}
