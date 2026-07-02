/************************************Copyright*****************************************
 *  Project    : AKNet
 *  Web        : https://github.com/1426186059/AKNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/07/02 18:05:45
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using AKNet.Common;
using System;
using System.Net;
using System.Net.Sockets;

namespace AKNet.Tcp3.Client
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

            NetLog.Log($"{NetType.Tcp3.ToString()} 客户端 正在连接服务器: {mIPEndPoint}");
            try
            {
                mSocket.BeginConnect(mIPEndPoint, ConnectCallback, this);
            }
            catch (Exception e)
            {
                DisConnectedWithException(e);
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
                try
                {
                    mSocket.BeginDisconnect(false, DisconnectCallback, this);
                }
                catch (Exception e)
                {
                    DisConnectedWithException(e);
                }
            }
            else { SetSocketState(SOCKET_PEER_STATE.DISCONNECTED); }
            return GetSocketState() == SOCKET_PEER_STATE.DISCONNECTED;
        }

        // ---------- 连接回调 ----------
        private static void ConnectCallback(IAsyncResult ar)
        {
            var self = (NetClientMain)ar.AsyncState;
            self.ProcessConnect(ar);
        }

        private void ProcessConnect(IAsyncResult ar)
        {
            try
            {
                mSocket.EndConnect(ar);
                NetLog.Log($"{NetType.Tcp3.ToString()} 客户端 连接服务器: {mIPEndPoint} 成功");
                SetSocketState(SOCKET_PEER_STATE.CONNECTED);
                // 启动接收
                if (!bIsReceiving)
                {
                    bIsReceiving = true;
                    StartReceive();
                }
            }
            catch (SocketException e)
            {
                NetLog.LogError($"{NetType.Tcp3.ToString()} 客户端 连接服务器: {mIPEndPoint} 失败：{e.SocketErrorCode}");
                if (mConfigInstance.bAutoReConnect) SetSocketState(SOCKET_PEER_STATE.RECONNECTING);
                else SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            }
            catch (Exception e)
            {
                DisConnectedWithException(e);
            }
        }

        // ---------- 断连回调 ----------
        private static void DisconnectCallback(IAsyncResult ar)
        {
            var self = (NetClientMain)ar.AsyncState;
            self.ProcessDisconnect(ar);
        }

        private void ProcessDisconnect(IAsyncResult ar)
        {
            try
            {
                mSocket.EndDisconnect(ar);
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
                NetLog.Log("客户端 主动 断开服务器 Finish");
            }
            catch (Exception e)
            {
                DisConnectedWithException(e);
            }
        }

        // ---------- 接收（BeginReceive 递归） ----------
        private void StartReceive()
        {
            if (mSocket == null || mSocketPeerState != SOCKET_PEER_STATE.CONNECTED)
            {
                bIsReceiving = false;
                return;
            }

            try
            {
                mSocket.BeginReceive(mReceiveBuffer, 0, mReceiveBuffer.Length, SocketFlags.None, ReceiveCallback, this);
            }
            catch (Exception e)
            {
                bIsReceiving = false;
                DisConnectedWithException(e);
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            var self = (NetClientMain)ar.AsyncState;
            self.ProcessReceive(ar);
        }

        private void ProcessReceive(IAsyncResult ar)
        {
            int bytesRead = 0;
            try
            {
                bytesRead = mSocket.EndReceive(ar);
            }
            catch (SocketException e)
            {
                bIsReceiving = false;
                DisConnectedWithSocketError(e.SocketErrorCode);
                return;
            }
            catch (Exception e)
            {
                bIsReceiving = false;
                DisConnectedWithException(e);
                return;
            }

            if (bytesRead > 0)
            {
                MultiThreadingReceiveSocketStream(bytesRead);
                // 递归继续接收
                StartReceive();
            }
            else
            {
                bIsReceiving = false;
                DisConnectedWithNormal();
            }
        }

        private void MultiThreadingReceiveSocketStream(int bytesRead)
        {
            lock (mReceiveStreamList)
            {
                mReceiveStreamList.WriteFrom(new ReadOnlySpan<byte>(mReceiveBuffer, 0, bytesRead));
            }
        }

        // ---------- 发送（BeginSend 递归） ----------
        public void SendNetStream(ReadOnlySpan<byte> mBufferSegment)
        {
            ResetSendHeartBeatTime();
            lock (mSendStreamList) { mSendStreamList.WriteFrom(mBufferSegment); }

            if (!bIsSending)
            {
                bIsSending = true;
                TrySendNext();
            }
        }

        /// <summary>
        /// 从发送缓冲区取数据，调用 BeginSend
        /// </summary>
        private void TrySendNext()
        {
            if (mSocket == null || mSocketPeerState != SOCKET_PEER_STATE.CONNECTED)
            {
                bIsSending = false;
                return;
            }

            int nLength = mSendStreamList.Length;
            if (nLength <= 0)
            {
                bIsSending = false;
                return;
            }

            nLength = Math.Min(mSendBuffer.Length, nLength);
            lock (mSendStreamList) { mSendStreamList.CopyTo(mSendBuffer.AsSpan(0, nLength)); }

            try
            {
                mSocket.BeginSend(mSendBuffer, 0, nLength, SocketFlags.None, SendCallback, this);
            }
            catch (Exception e)
            {
                bIsSending = false;
                DisConnectedWithException(e);
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            var self = (NetClientMain)ar.AsyncState;
            self.ProcessSend(ar);
        }

        private void ProcessSend(IAsyncResult ar)
        {
            int bytesSent = 0;
            try
            {
                bytesSent = mSocket.EndSend(ar);
            }
            catch (SocketException e)
            {
                bIsSending = false;
                DisConnectedWithSocketError(e.SocketErrorCode);
                return;
            }
            catch (Exception e)
            {
                bIsSending = false;
                DisConnectedWithException(e);
                return;
            }

            if (bytesSent > 0)
            {
                lock (mSendStreamList) { mSendStreamList.ClearBuffer(bytesSent); }
                // 继续发送剩余数据
                TrySendNext();
            }
            else
            {
                bIsSending = false;
                DisConnectedWithNormal();
            }
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
                System.Threading.ThreadPool.UnsafeQueueUserWorkItem(static s => { try { ((Socket)s).Close(); } catch { } }, mSocket2);
            }
        }
    }
}
