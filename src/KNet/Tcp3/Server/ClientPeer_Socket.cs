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
using System.Net;
using System.Net.Sockets;

namespace KNet.Tcp3.Server
{
    internal partial class ClientPeer
    {
        public void HandleConnectedSocket(Socket otherSocket)
        {
            MainThreadCheck.Check();
            if (bStreamsDirty)
            {
                lock (mReceiveStreamList) { mReceiveStreamList.Reset(); }
                lock (mSendStreamList) { mSendStreamList.Reset(); }
                bStreamsDirty = false;
            }
            this.mSocket = otherSocket;
            SetSocketState(SOCKET_PEER_STATE.CONNECTED);
            bIsSending = false;
            bIsReceiving = false;

            // 启动接收
            if (!bIsReceiving)
            {
                bIsReceiving = true;
                StartReceive();
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
            var self = (ClientPeer)ar.AsyncState;
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
                StartReceive();
            }
            else
            {
                bIsReceiving = false;
                DisConnectedWithNormal();
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
            else
            {
                if (!bIsSending && mSendStreamList.Length > 0)
                    throw new Exception("SendNetStream 有数据, 但发送不了啊");
            }
        }

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
            var self = (ClientPeer)ar.AsyncState;
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
                TrySendNext();
            }
            else
            {
                bIsSending = false;
                DisConnectedWithNormal();
            }
        }

        public IPEndPoint GetIPEndPoint()
        {
            IPEndPoint mRemoteEndPoint = null;
            try { if (mSocket != null && mSocket.RemoteEndPoint != null) mRemoteEndPoint = mSocket.RemoteEndPoint as IPEndPoint; } catch { }
            return mRemoteEndPoint;
        }

        private void DisConnectedWithNormal() { SetSocketState(SOCKET_PEER_STATE.DISCONNECTED); }
        private void DisConnectedWithException(Exception e) { SetSocketState(SOCKET_PEER_STATE.DISCONNECTED); }
        private void DisConnectedWithSocketError(SocketError mError) { SetSocketState(SOCKET_PEER_STATE.DISCONNECTED); }

        void CloseSocket()
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
