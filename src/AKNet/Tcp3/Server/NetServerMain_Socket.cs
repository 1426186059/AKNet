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
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace AKNet.Tcp3.Server
{
    internal partial class NetServerMain
    {
        public void InitNet()
        {
            List<int> mPortList = IPAddressHelper.GetAvailableTcpPortList();
            int nTryBindCount = 100;
            while (nTryBindCount-- > 0)
            {
                if (mPortList.Count > 0)
                {
                    int nPort = mPortList[RandomTool.RandomArrayIndex(0, mPortList.Count)];
                    InitNet(nPort);
                    mPortList.Remove(nPort);
                    if (GetServerState() == SOCKET_SERVER_STATE.NORMAL)
                    {
                        break;
                    }
                }
            }

            if (GetServerState() != SOCKET_SERVER_STATE.NORMAL)
            {
                NetLog.LogError("Tcp3 Server 自动查找可用端口 失败！！！");
            }
        }

        public void InitNet(int nPort)
        {
            InitNet(IPAddress.Any, nPort);
        }

        public void InitNet(string Ip, int nPort)
        {
            InitNet(IPAddress.Parse(Ip), nPort);
        }

        private void InitNet(IPAddress mIPAddress, int nPort)
        {
            CloseNet();
            try
            {
                this.nPort = nPort;
                mState = SOCKET_SERVER_STATE.NORMAL;
                IPEndPoint localEndPoint = new IPEndPoint(mIPAddress, nPort);

                this.mListenSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                this.mListenSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
                this.mListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                this.mListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, int.MaxValue);
                this.mListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, int.MaxValue);
                this.mListenSocket.Bind(localEndPoint);
                this.mListenSocket.Listen(this.mConfigInstance.MaxPlayerCount);

                NetLog.Log($"{NetType.Tcp3.ToString()} 服务器 初始化成功: {localEndPoint}");
                StartAccept();
            }
            catch (SocketException ex)
            {
                mState = SOCKET_SERVER_STATE.EXCEPTION;
                NetLog.LogError(ex.SocketErrorCode + " | " + ex.Message + " | " + ex.StackTrace);
                NetLog.LogError($"{NetType.Tcp3.ToString()} 服务器 初始化失败: {mIPAddress} | {nPort}");
            }
            catch (Exception ex)
            {
                mState = SOCKET_SERVER_STATE.EXCEPTION;
                NetLog.LogError(ex.Message + " | " + ex.StackTrace);
                NetLog.LogError($"{NetType.Tcp3.ToString()} 服务器 初始化失败: {mIPAddress} | {nPort}");
            }
        }

        public int GetPort()
        {
            return this.nPort;
        }

        public SOCKET_SERVER_STATE GetServerState()
        {
            return mState;
        }

        // ---------- BeginAccept 递归 ----------
        private void StartAccept()
        {
            if (mListenSocket == null) return;

            try
            {
                mListenSocket.BeginAccept(AcceptCallback, this);
            }
            catch (Exception e)
            {
                if (mListenSocket != null)
                {
                    NetLog.LogException(e);
                }
            }
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            var self = (NetServerMain)ar.AsyncState;
            self.ProcessAccept(ar);
        }

        private void ProcessAccept(IAsyncResult ar)
        {
            Socket mClientSocket = null;
            try
            {
                mClientSocket = mListenSocket.EndAccept(ar);
            }
            catch (Exception e)
            {
                NetLog.LogError("ProcessAccept: " + e.Message);
                StartAccept();
                return;
            }

            if (mClientSocket != null)
            {
#if DEBUG
                NetLog.Assert(mClientSocket != null);
#endif
                if (!MultiThreadingHandleConnectedSocket(mClientSocket))
                {
                    HandleConnectFull(mClientSocket);
                }
            }

            // 递归继续 Accept
            StartAccept();
        }

        private void HandleConnectFull(Socket mClientSocket)
        {
            try
            {
                mClientSocket.Shutdown(SocketShutdown.Both);
            }
            catch
            {

            }
            finally
            {
                mClientSocket.Close();
            }
        }

        public void CloseNet()
        {
            MainThreadCheck.Check();
            if (mListenSocket != null)
            {
                Socket mSocket = mListenSocket;
                mListenSocket = null;
                try
                {
                    mSocket.Close();
                }
                catch { }
            }
        }
    }
}
