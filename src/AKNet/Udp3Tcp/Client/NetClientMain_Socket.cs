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
using AKNet.Udp3Tcp.Common;
using System;
using System.Net;
using System.Net.Sockets;

namespace AKNet.Udp3Tcp.Client
{
    internal partial class NetClientMain
    {
        public void ConnectServer(string ip, int nPort)
        {
            this.ServerPort = nPort;
            this.ServerIp = ip;
            this.remoteEndPoint = new IPEndPoint(IPAddress.Parse(ServerIp), ServerPort);
            this.ReceiveArgs.RemoteEndPoint = remoteEndPoint;
            this.SendArgs.RemoteEndPoint = remoteEndPoint;
            this.ConnectServer();
            this.StartReceiveEventArg();
        }

        public void ConnectServer()
        {
            SendConnect();
        }

        public void ReConnectServer()
        {
            SendConnect();
        }

        public IPEndPoint GetIPEndPoint()
        {
            return remoteEndPoint;
        }

        public bool DisConnectServer()
        {
            if (mSocketPeerState == SOCKET_PEER_STATE.CONNECTED || mSocketPeerState == SOCKET_PEER_STATE.CONNECTING)
            {
                SendDisConnect();
                return false;
            }
            else
            {
                return true;
            }
        }

        private void StartReceiveEventArg()
        {
            while (true)
            {
                bool bIOPending = false;
                if (mSocket != null)
                {
                    try
                    {
                        bIOPending = mSocket.ReceiveFromAsync(ReceiveArgs);
                    }
                    catch (Exception e)
                    {
                        bReceiveIOContexUsed = false;
                        DisConnectedWithException(e);
                    }
                }
                else
                {
                    bReceiveIOContexUsed = false;
                }

                if (bIOPending) break;

                ProcessReceive(null, ReceiveArgs);
            }
        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(sender, e);
            StartReceiveEventArg();
        }

        private void ProcessReceive(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                MultiThreading_ReceiveWaitCheckNetPackage(e);
            }
        }

        // ------------------ 发送 ------------------

        private void StartSendEventArg()
        {
            while (true)
            {
                bool bIOPending = false;

                if (mSocket != null)
                {
                    try
                    {
                        bIOPending = mSocket.SendToAsync(SendArgs);
                    }
                    catch (Exception e)
                    {
                        bSendIOContexUsed = false;
                        DisConnectedWithException(e);
                    }
                }
                else
                {
                    bSendIOContexUsed = false;
                }

                if (bIOPending) break;

                if (!ProcessSendSync(null, SendArgs)) break;
            }
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (ProcessSendSync(sender, e))
                StartSendEventArg();
        }

        // true=还有数据要继续发
        private bool ProcessSendSync(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                return SendNetStream2();
            }
            else
            {
                bSendIOContexUsed = false;
                DisConnectedWithSocketError(e.SocketError);
                return false;
            }
        }

        private void SendNetPackage2(NetUdpSendFixedSizePackage mPackage)
        {
            MainThreadCheck.Check();

            lock (mSendStreamList)
            {
                var mBufferItem = mSendStreamList.BeginSpan();
                mBufferItem.AddSpan(UdpPackageEncryption.EncodeHead(mPackage));
                if (mPackage.WindowBuff != null)
                {
                    mPackage.WindowBuff.CopyTo(mBufferItem.GetCanWriteSpan(), mPackage.WindowOffset, mPackage.WindowLength);
                    mBufferItem.nSpanLength += mPackage.WindowLength;
                }
                mSendStreamList.FinishSpan();
            }

            if (!bSendIOContexUsed)
            {
                bSendIOContexUsed = true;
                if (SendNetStream2())
                    StartSendEventArg();
            }
        }

        // true=还有数据要发（调用方需调 StartSendEventArg 继续）
        // false=发完了
        private bool SendNetStream2()
        {
            var mSendArgSpan = SendArgs.Buffer.AsSpan();
            int nSendBytesCount = 0;
            lock (mSendStreamList)
            {
                nSendBytesCount += mSendStreamList.WriteToMax(mSendArgSpan);
            }

            if (nSendBytesCount > 0)
            {
                nLastSendBytesCount = nSendBytesCount;
                SendArgs.SetBuffer(0, nSendBytesCount);
                return true;
            }
            else
            {
                bSendIOContexUsed = false;
                return false;
            }
        }

        public void DisConnectedWithNormal()
        {
            NetLog.Log("客户端 正常 断开服务器 ");
            SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
        }

        private void DisConnectedWithException(Exception e)
        {
            if (mSocket != null)
            {
                NetLog.LogException(e);
            }
            DisConnectedWithError();
        }

        private void DisConnectedWithSocketError(SocketError e)
        {
            DisConnectedWithError();
        }

        private void DisConnectedWithError()
        {
            if (mSocketPeerState == SOCKET_PEER_STATE.DISCONNECTING)
            {
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            }
            else if (mSocketPeerState == SOCKET_PEER_STATE.CONNECTED || mSocketPeerState == SOCKET_PEER_STATE.CONNECTING)
            {
                if (mConfigInstance.bAutoReConnect)
                {
                    SetSocketState(SOCKET_PEER_STATE.RECONNECTING);
                }
                else
                {
                    SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
                }
            }
        }

        private void CloseSocket()
        {
            if (mSocket != null)
            {
                Socket mSocket2 = mSocket;
                mSocket = null;

                try
                {
                    mSocket2.Close();
                }
                catch (Exception) { }
            }
        }
    }
}
