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
using System.Threading.Tasks;

namespace AKNet.WebSocket.Client
{
    internal partial class NetClientMain
    {
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
            else
            {
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            }
            NetLog.Log("WebSocket 客户端 主动 断开服务器 Finish......");
            return GetSocketState() == SOCKET_PEER_STATE.DISCONNECTED;
        }

        public void SendNetStream(ReadOnlySpan<byte> mBufferSegment)
        {
            ResetSendHeartBeatTime();

            lock (mSendStreamList)
            {
                mSendStreamList.WriteFrom(mBufferSegment);
            }

            if (!bSending)
            {
                bSending = true;
                SendNetStream1();
            }
        }

        private void SendNetStream1()
        {
            while (true)
            {
                int nLength;
                lock (mSendStreamList) { nLength = mSendStreamList.Length; }
                if (nLength <= 0) { bSending = false; return; }

                ClientWebSocket ws;
                lock (mWsLock) { ws = mWebSocket; }
                if (ws == null || ws.State != WebSocketState.Open) { bSending = false; return; }

                byte[] tempBuf = new byte[Math.Min(mSendBuffer.Length, nLength)];
                lock (mSendStreamList) { mSendStreamList.CopyTo(new Span<byte>(tempBuf, 0, tempBuf.Length)); }

                try
                {
                    ws.SendAsync(new ArraySegment<byte>(tempBuf, 0, tempBuf.Length),
                        WebSocketMessageType.Binary, true, System.Threading.CancellationToken.None)
                        .GetAwaiter().GetResult();

                    lock (mSendStreamList) { mSendStreamList.ClearBuffer(tempBuf.Length); }
                }
                catch (InvalidOperationException)
                {
                    // 收发冲突，下帧重试
                    bSending = false;
                    return;
                }
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
    }
}
