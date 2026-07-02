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

namespace AKNet.LinuxTcp.Client
{
    internal partial class NetClientMain
    {
        public void ConnectServer(string ip, int nPort)
        {
            this.ServerPort = nPort;
            this.ServerIp = ip;
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), nPort);
            ReceiveArgs.RemoteEndPoint = remoteEndPoint;
            SendArgs.RemoteEndPoint = remoteEndPoint;

            ConnectServer();
            StartReceiveEventArg();
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
            var mSocketPeerState = GetSocketState();
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
            bool bIOSyncCompleted = false;
            if (mSocket != null)
            {
                try
                {
                    bIOSyncCompleted = !mSocket.ReceiveFromAsync(ReceiveArgs);
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

            if (bIOSyncCompleted)
            {
                System.Threading.Tasks.Task.Run(() => ProcessReceive(null, ReceiveArgs));
            }
        }

        private void ProcessReceive(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                MultiThreading_ReceiveWaitCheckNetPackage(e);
            }
            StartReceiveEventArg();
        }

        private void StartSendEventArg()
        {
            bool bIOSyncCompleted = false;
            if (mSocket != null)
            {
                try
                {
                    bIOSyncCompleted = !mSocket.SendToAsync(SendArgs);
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

            if (bIOSyncCompleted)
            {
                System.Threading.Tasks.Task.Run(() => ProcessSend(null, SendArgs));
            }
        }

        private void ProcessSend(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                SendNetStream2(e.BytesTransferred);
            }
            else
            {
                bSendIOContexUsed = false;
                DisConnectedWithSocketError(e.SocketError);
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
                StartSendEventArg();
            }
            else
            {
                bSendIOContexUsed = false;
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
            var mSocketPeerState = GetSocketState();
            if (mSocketPeerState == SOCKET_PEER_STATE.DISCONNECTING)
            {
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            }
            else if (mSocketPeerState == SOCKET_PEER_STATE.CONNECTED || mSocketPeerState == SOCKET_PEER_STATE.CONNECTING)
            {
                SetSocketState(SOCKET_PEER_STATE.RECONNECTING);
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
