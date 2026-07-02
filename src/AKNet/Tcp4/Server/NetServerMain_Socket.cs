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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AKNet.Tcp4.Server
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
                NetLog.LogError("Tcp4(Socket async) Server 自动查找可用端口 失败！！！");
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

                mListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                mListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, int.MaxValue);
                mListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, int.MaxValue);
                mListenSocket.Bind(localEndPoint);
                mListenSocket.Listen(this.mConfigInstance.MaxPlayerCount);

                NetLog.Log($"Tcp4(Socket async) 服务器 初始化成功: {localEndPoint}");

                mListenCts = new System.Threading.CancellationTokenSource();
                var ct = mListenCts.Token;
                _ = Task.Run(() => AcceptLoop(ct));
            }
            catch (SocketException ex)
            {
                mState = SOCKET_SERVER_STATE.EXCEPTION;
                NetLog.LogError(ex.SocketErrorCode + " | " + ex.Message + " | " + ex.StackTrace);
                NetLog.LogError($"Tcp4(Socket async) 服务器 初始化失败: {mIPAddress} | {nPort}");
            }
            catch (Exception ex)
            {
                mState = SOCKET_SERVER_STATE.EXCEPTION;
                NetLog.LogError(ex.Message + " | " + ex.StackTrace);
                NetLog.LogError($"Tcp4(Socket async) 服务器 初始化失败: {mIPAddress} | {nPort}");
            }
        }

        private async Task AcceptLoop(System.Threading.CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && mListenSocket != null)
            {
                try
                {
                    Socket client = await mListenSocket.AcceptAsync().ConfigureAwait(false);
                    if (!MultiThreadingHandleConnectedSocket(client))
                    {
                        HandleConnectFull(client);
                    }
                }
                catch (System.OperationCanceledException) { break; }
                catch (ObjectDisposedException) { break; }
                catch (Exception e)
                {
                    if (mListenSocket != null)
                    {
                        NetLog.LogException(e);
                    }
                }
            }
        }

        private void HandleConnectFull(Socket client)
        {
            try
            {
                client.Shutdown(SocketShutdown.Both);
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

            // 先关闭 listen socket 让 AcceptAsync 抛出 ObjectDisposedException 退出循环
            if (mListenSocket != null)
            {
                try { mListenSocket.Close(); } catch { }
                mListenSocket = null;
            }

            try { mListenCts?.Cancel(); } catch { }

            if (mListenCts != null)
            {
                try { mListenCts.Dispose(); } catch { }
                mListenCts = null;
            }

            // 清除等待队列中的连接
            lock (mConnectSocketQueue)
            {
                while (mConnectSocketQueue.TryDequeue(out Socket client))
                {
                    try { client.Close(); } catch { }
                }
            }
        }
    }
}
