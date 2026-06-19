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
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AKNet.Tcp.Client
{
    internal partial class NetClientMain
    {
        public void ReConnectServer()
        {
            bool Connected = false;
            try { Connected = mSocket != null && mSocket.Connected; } catch { }
            if (Connected) SetSocketState(SOCKET_PEER_STATE.CONNECTED);
            else ConnectServer(this.ServerIp, this.nServerPort);
        }

        public void ConnectServer(string ServerAddr, int ServerPort)
        {
            Reset();
            if (bStreamsDirty)
            {
                lock (mSendStreamList) { mSendStreamList.Reset(); }
                lock (mReceiveStreamList) { mReceiveStreamList.Reset(); }
                bStreamsDirty = false;
            }
            this.ServerIp = ServerAddr;
            this.nServerPort = ServerPort;
            SetSocketState(SOCKET_PEER_STATE.CONNECTING);
            mSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            mSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, int.MaxValue);
            mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, int.MaxValue);

            if (mIPEndPoint == null) { IPAddress mIPAddress = IPAddress.Parse(ServerAddr); mIPEndPoint = new IPEndPoint(mIPAddress, ServerPort); }

            if (!bConnectIOContexUsed)
            {
                bConnectIOContexUsed = true;
                mConnectIOContex.RemoteEndPoint = mIPEndPoint;
                NetLog.Log($"{NetType.TCP.ToString()} 客户端 正在连接服务器: {mIPEndPoint}");
                StartConnectEventArg();
            }
        }

        public bool DisConnectServer()
        {
            NetLog.Log("客户端 主动 断开服务器 Begin......");
            MainThreadCheck.Check();
            bool Connected = false;
            try { Connected = mSocket != null && mSocket.Connected; } catch { }
            if (Connected)
            {
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTING);
                mDisConnectIOContex.RemoteEndPoint = mIPEndPoint;
                if (!bDisConnectIOContexUsed) { bDisConnectIOContexUsed = true; StartDisconnectEventArg(); }
            }
            else { SetSocketState(SOCKET_PEER_STATE.DISCONNECTED); bDisConnectIOContexUsed = false; }
            return GetSocketState() == SOCKET_PEER_STATE.DISCONNECTED;
        }

        // ---------- 连接/断连（低频，保持 Task.Run 即可） ----------
        private void StartConnectEventArg()
        {
            bool bIOSyncCompleted = false;
            if (mSocket != null) { try { bIOSyncCompleted = !mSocket.ConnectAsync(mConnectIOContex); } catch (Exception e) { bConnectIOContexUsed = false; DisConnectedWithException(e); } }
            else { bConnectIOContexUsed = false; }
            if (bIOSyncCompleted)
#if NET8_0_OR_GREATER
                ThreadPool.UnsafeQueueUserWorkItem<ValueTuple<NetClientMain, SocketAsyncEventArgs>>(
                    static state => state.Item1.ProcessConnect(state.Item2),
                    (this, mConnectIOContex), false);
#else
                Task.Run(() => this.ProcessConnect(mConnectIOContex));
#endif
        }

        private void StartDisconnectEventArg()
        {
            bool bIOSyncCompleted = false;
            if (mSocket != null) { try { bIOSyncCompleted = !mSocket.DisconnectAsync(mDisConnectIOContex); } catch (Exception e) { bDisConnectIOContexUsed = false; DisConnectedWithException(e); } }
            else { bDisConnectIOContexUsed = false; }
            if (bIOSyncCompleted)
#if NET8_0_OR_GREATER
                ThreadPool.UnsafeQueueUserWorkItem<ValueTuple<NetClientMain, SocketAsyncEventArgs>>(
                    static state => state.Item1.ProcessDisconnect(state.Item2),
                    (this, mDisConnectIOContex), false);
#else
                Task.Run(() => this.ProcessDisconnect(mDisConnectIOContex));
#endif
        }

        private void ProcessConnect(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                NetLog.Log($"{NetType.TCP.ToString()} 客户端 连接服务器: {mIPEndPoint} 成功");
                SetSocketState(SOCKET_PEER_STATE.CONNECTED);
                if (!bReceiveIOContextUsed) { bReceiveIOContextUsed = true; StartReceiveEventArg(); }
            }
            else
            {
                if (mConfigInstance.bAutoReConnect) SetSocketState(SOCKET_PEER_STATE.RECONNECTING);
                else SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
                NetLog.LogError($"{NetType.TCP.ToString()} 客户端 连接服务器: {mIPEndPoint} 失败：{e.SocketError}");
            }
            e.RemoteEndPoint = null;
            bConnectIOContexUsed = false;
        }

        private void ProcessDisconnect(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success) { SetSocketState(SOCKET_PEER_STATE.DISCONNECTED); NetLog.Log("客户端 主动 断开服务器 Finish"); }
            else { DisConnectedWithSocketError(e.SocketError); }
            e.RemoteEndPoint = null;
            bDisConnectIOContexUsed = false;
        }

        // ---------- 接收（高频，while 循环） ----------
        private void StartReceiveEventArg()
        {
            while (true)
            {
                bool bIOPending = false;
                if (mSocket != null)
                {
                    try { bIOPending = mSocket.ReceiveAsync(mReceiveIOContex); }
                    catch (Exception e) { bReceiveIOContextUsed = false; DisConnectedWithException(e); }
                }
                else { bReceiveIOContextUsed = false; }

                if (!bIOPending) ProcessReceive(mReceiveIOContex);
                else break;
            }
        }

        private void OnIOCompleted_Receive(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
            StartReceiveEventArg();
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0) { MultiThreadingReceiveSocketStream(e); }
                else { bReceiveIOContextUsed = false; DisConnectedWithNormal(); }
            }
            else { bReceiveIOContextUsed = false; DisConnectedWithSocketError(e.SocketError); }
        }

        // ---------- 发送（高频，while 循环） ----------
        private void StartSendEventArg()
        {
            while (true)
            {
                bool bIOPending = false;
                if (mSocket != null)
                {
                    try { bIOPending = mSocket.SendAsync(mSendIOContex); }
                    catch (Exception e) { bSendIOContextUsed = false; DisConnectedWithException(e); }
                }
                else { bSendIOContextUsed = false; }

                if (!bIOPending) { if (!ProcessSendSync(mSendIOContex)) break; }
                else break;
            }
        }

        private void OnIOCompleted_Send(object sender, SocketAsyncEventArgs e)
        {
            if (ProcessSendSync(e)) StartSendEventArg();
        }

        // true=还有数据要继续发
        private bool ProcessSendSync(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0) return SendLoopChunk(e.BytesTransferred);
                else { DisConnectedWithNormal(); bSendIOContextUsed = false; return false; }
            }
            else { DisConnectedWithSocketError(e.SocketError); bSendIOContextUsed = false; return false; }
        }

        public void SendNetStream(ReadOnlySpan<byte> mBufferSegment)
        {
            ResetSendHeartBeatTime();
            lock (mSendStreamList) { mSendStreamList.WriteFrom(mBufferSegment); }
            if (!bSendIOContextUsed) { bSendIOContextUsed = true;
#if NET8_0_OR_GREATER
                ThreadPool.UnsafeQueueUserWorkItem<ValueTuple<NetClientMain, int>>(
                    static state => { if (state.Item1.SendLoopChunk(state.Item2)) state.Item1.StartSendEventArg(); },
                    (this, 0), false);
#else
                Task.Run(() => { if (SendLoopChunk(0)) StartSendEventArg(); });
#endif
            }
        }

        // true=还有数据要继续发（调用方需调 StartSendEventArg 继续）
        private bool SendLoopChunk(int BytesTransferred = 0)
        {
            if (BytesTransferred > 0) { lock (mSendStreamList) { mSendStreamList.ClearBuffer(BytesTransferred); } }

            int nLength = mSendStreamList.Length;
            if (nLength > 0)
            {
                nLength = Math.Min(mSendIOContex.MemoryBuffer.Length, nLength);
                lock (mSendStreamList) { mSendStreamList.CopyTo(mSendIOContex.MemoryBuffer.Span.Slice(0, nLength)); }
                mSendIOContex.SetBuffer(0, nLength);
                return true;
            }
            else { bSendIOContextUsed = false; return false; }
        }

        // ---------- 错误处理 ----------
        private void DisConnectedWithNormal()
        {
#if DEBUG
            NetLog.Log("客户端 正常 断开服务器 ");
#endif
            DisConnectedWithError();
        }

        private void DisConnectedWithException(Exception e)
        {
#if DEBUG
            if (mSocket != null) NetLog.LogException(e);
#endif
            DisConnectedWithError();
        }

        private void DisConnectedWithSocketError(SocketError mError)
        {
#if DEBUG
            NetLog.LogError(mError);
#endif
            DisConnectedWithError();
        }

        private void DisConnectedWithError()
        {
            var mSocketPeerState = GetSocketState();
            if (mSocketPeerState == SOCKET_PEER_STATE.DISCONNECTING)
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            else if (mSocketPeerState == SOCKET_PEER_STATE.CONNECTED)
            {
                if (mConfigInstance.bAutoReConnect) SetSocketState(SOCKET_PEER_STATE.RECONNECTING);
                else SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            }
        }

        public IPEndPoint GetIPEndPoint()
        {
            IPEndPoint mRemoteEndPoint = null;
            try { if (mSocket != null && mSocket.RemoteEndPoint != null) mRemoteEndPoint = mSocket.RemoteEndPoint as IPEndPoint; } catch { }
            return mRemoteEndPoint;
        }

        private void CloseSocket()
        {
            if (mSocket != null)
            {
                Socket mSocket2 = mSocket;
                mSocket = null;
                try { mSocket2.Close(); } catch { }
            }
        }

        private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect: ProcessConnect(e); break;
                case SocketAsyncOperation.Disconnect: ProcessDisconnect(e); break;
                case SocketAsyncOperation.Receive: ProcessReceive(e); StartReceiveEventArg(); break;
                case SocketAsyncOperation.Send: if (ProcessSendSync(e)) StartSendEventArg(); break;
                default: NetLog.LogError("The last operation completed on the socket was not a receive or send"); break;
            }
        }
    }
}
