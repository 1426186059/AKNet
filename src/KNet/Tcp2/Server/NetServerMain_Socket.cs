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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace KNet.Tcp2.Server
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
                NetLog.LogError("Tcp2(TcpListener) Server 自动查找可用端口 失败！！！");
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

                this.mTcpListener = new TcpListener(localEndPoint);
                this.mTcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                this.mTcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, int.MaxValue);
                this.mTcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, int.MaxValue);
                this.mTcpListener.Start(this.mConfigInstance.MaxPlayerCount);

                NetLog.Log($"Tcp2(TcpListener) 服务器 初始化成功: {localEndPoint}");

                mListenCts = new System.Threading.CancellationTokenSource();
                var ct = mListenCts.Token;
                _ = System.Threading.Tasks.Task.Run(() => AcceptLoop(ct));
            }
            catch (SocketException ex)
            {
                mState = SOCKET_SERVER_STATE.EXCEPTION;
                NetLog.LogError(ex.SocketErrorCode + " | " + ex.Message + " | " + ex.StackTrace);
                NetLog.LogError($"Tcp2(TcpListener) 服务器 初始化失败: {mIPAddress} | {nPort}");
            }
            catch (Exception ex)
            {
                mState = SOCKET_SERVER_STATE.EXCEPTION;
                NetLog.LogError(ex.Message + " | " + ex.StackTrace);
                NetLog.LogError($"Tcp2(TcpListener) 服务器 初始化失败: {mIPAddress} | {nPort}");
            }
        }

        private async System.Threading.Tasks.Task AcceptLoop(System.Threading.CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && mTcpListener != null)
            {
                try
                {
                    TcpClient client = await mTcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                    if (!MultiThreadingHandleConnectedTcpClient(client))
                    {
                        HandleConnectFull(client);
                    }
                }
                catch (System.OperationCanceledException) { break; }
                catch (ObjectDisposedException) { break; }
                catch (Exception e)
                {
                    if (mTcpListener != null)
                    {
                        NetLog.LogException(e);
                    }
                }
            }
        }

        private void HandleConnectFull(TcpClient client)
        {
            try
            {
                client.GetStream()?.Close();
                client.Close();
            }
            catch { }
        }

        public int GetPort()
        {
            return this.nPort;
        }

        public SOCKET_SERVER_STATE GetServerState()
        {
            return mState;
        }

        public void CloseNet()
        {
            MainThreadCheck.Check();

            try { mListenCts?.Cancel(); } catch { }

            if (mTcpListener != null)
            {
                try { mTcpListener.Stop(); } catch { }
                mTcpListener = null;
            }

            if (mListenCts != null)
            {
                try { mListenCts.Dispose(); } catch { }
                mListenCts = null;
            }

            // 清除等待队列中的连接
            lock (mConnectTcpClientQueue)
            {
                while (mConnectTcpClientQueue.TryDequeue(out TcpClient client))
                {
                    try { client.Close(); } catch { }
                }
            }
        }
    }
}
