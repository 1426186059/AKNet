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
using System.Threading;
using System.Threading.Tasks;

namespace KNet.WebSocket.Server
{
    public partial class NetServerMain : NetServerInterface
    {
        // 传输层：原生 TcpListener + 自建 RFC6455 握手
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
                NetLog.LogError("WebSocket Server 自动查找可用端口 失败！！！");
            }
        }

        public void InitNet(int nPort)
        {
            InitNet(IPAddress.Any.ToString(), nPort);
        }

        public void InitNet(string Ip, int nPort)
        {
            CloseNet();
            try
            {
                this.nPort = nPort;
                this.mBindIp = Ip;
                mState = SOCKET_SERVER_STATE.NORMAL;

                this.mTcpListener = new TcpListener(IPAddress.Parse(Ip), nPort);
                this.mTcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                this.mTcpListener.Start(this.mConfigInstance.MaxPlayerCount);

                NetLog.Log($"WebSocket 服务器 初始化成功: {Ip}:{nPort}");
                mCancellationTokenSource = new CancellationTokenSource();
                mAcceptTask = AcceptLoopAsync();
            }
            catch (Exception ex)
            {
                mState = SOCKET_SERVER_STATE.EXCEPTION;
                NetLog.LogError($"WebSocket 服务器 初始化失败: {Ip} | {nPort} | {ex.Message}");
            }
        }

        private async Task AcceptLoopAsync()
        {
            var cancellationToken = mCancellationTokenSource.Token;
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var tcpClient = await mTcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                    var mClientSocket = tcpClient.Client;

                    if (!MultiThreadingHandleConnectedSocket(mClientSocket))
                    {
                        try
                        {
                            mClientSocket.Shutdown(SocketShutdown.Both);
                        }
                        catch { }
                        finally
                        {
                            mClientSocket.Close();
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                if (mTcpListener != null)
                {
                    NetLog.LogException(e);
                }
            }
        }
    }
}
