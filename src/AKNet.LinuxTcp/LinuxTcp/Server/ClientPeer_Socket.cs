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
using AKNet.LinuxTcp.Common;
using System;
using System.Net;
using System.Net.Sockets;

namespace AKNet.LinuxTcp.Server
{
    internal partial class ClientPeer
    {
        public void HandleConnectedSocket(FakeSocket mSocket)
        {
            MainThreadCheck.Check();
            NetLog.Assert(mSocket != null, "mSocket == null");

            this.mSocket = mSocket;
            this.mIPEndPoint = mSocket.RemoteEndPoint;
            SendArgs.RemoteEndPoint = this.mIPEndPoint;
            SetSocketState(SOCKET_PEER_STATE.CONNECTED);
            mSocket.SetClientPeer(this);
        }

        public IPEndPoint GetIPEndPoint()
        {
            if (mSocket != null)
            {
                return mSocket.RemoteEndPoint;
            }
            else
            {
                return mIPEndPoint;
            }
        }

        public sk_buff GetReceivePackage()
        {
            return mSocket.GetReceivePackage();
        }

        private bool SendToAsync(SocketAsyncEventArgs e)
        {
            bool bIOSyncCompleted = false;
            if (mSocket != null)
            {
                try
                {
                    bIOSyncCompleted = !mSocket.SendToAsync(e);
                }
                catch (Exception ex)
                {
                    bSendIOContexUsed = false;
                    if (mSocket != null)
                    {
                        NetLog.LogException(ex);
                    }
                }
            }
            return !bIOSyncCompleted;
        }

        private void ProcessSend(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                SendNetStream2(e.BytesTransferred);
            }
            else
            {
                NetLog.LogError(e.SocketError);
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
                bSendIOContexUsed = false;
            }
        }

        public void WriteToSendBuffer(ReadOnlySpan<byte> mPackage)
        {
            MainThreadCheck.Check();
            lock (mSendStreamList)
            {
                mSendStreamList.WriteFrom(mPackage);
            }
            if (!bSendIOContexUsed)
            {
                bSendIOContexUsed = true;
                SendNetStream2();
            }
        }

        private void SendNetStream2(int BytesTransferred = -1)
        {
            if (BytesTransferred >= 0)
            {
                if (BytesTransferred != nLastSendBytesCount)
                {
                    NetLog.LogError("UDP 发生短写");
                }
            }

            var mSendArgSpan = SendArgs.Buffer.AsSpan();
            int nSendBytesCount = 0;
            lock (mSendStreamList)
            {
                nSendBytesCount += mSendStreamList.WriteTo(mSendArgSpan);
            }

            if (nSendBytesCount > 0)
            {
                nLastSendBytesCount = nSendBytesCount;
                SendArgs.SetBuffer(0, nSendBytesCount);
                if (!SendToAsync(SendArgs))
                {
                    ProcessSend(null, SendArgs);
                }
            }
            else
            {
                bSendIOContexUsed = false;
            }
        }

        public void CloseSocket()
        {
            if (mSocket != null)
            {
                mSocket.Close();
                mSocket = null;
            }
        }
    }
}
