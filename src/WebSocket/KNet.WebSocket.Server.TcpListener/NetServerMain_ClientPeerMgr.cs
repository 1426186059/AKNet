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
using System.Net.Sockets;

namespace KNet.WebSocket.Server
{
    internal partial class NetServerMain : NetServerInterface
    {
        // ClientPeer 管理器：遍历活跃连接、心跳超时检测与移除
        public void Update(double elapsed)
        {
            if (elapsed >= 0.3)
            {
                NetLog.LogWarning("帧 时间 太长: " + elapsed);
            }

            while (CreateClientPeer())
            {
            }

            for (int i = mClientList.Count - 1; i >= 0; i--)
            {
                ClientPeerWrap mClientPeer = mClientList[i];
                if (mClientPeer.GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
                {
                    mClientPeer.Update(elapsed);
                }
                else
                {
                    mClientList.RemoveAt(i);
                    PrintRemoveClientMsg(mClientPeer);
                    mClientPeer.Reset();
                }
            }
        }

        public bool MultiThreadingHandleConnectedSocket(Socket mSocket)
        {
            int nNowConnectCount = mClientList.Count + mConnectSocketQueue.Count;
            if (nNowConnectCount >= this.mConfigInstance.MaxPlayerCount)
            {
#if DEBUG
                NetLog.Log($"WebSocket 服务器爆满, 客户端总数: {nNowConnectCount}");
#endif
                return false;
            }
            else
            {
                lock (mConnectSocketQueue)
                {
                    mConnectSocketQueue.Enqueue(mSocket);
                }
                return true;
            }
        }

        private bool CreateClientPeer()
        {
            Socket mSocket = null;
            lock (mConnectSocketQueue)
            {
                mConnectSocketQueue.TryDequeue(out mSocket);
            }
            if (mSocket != null)
            {
                ClientPeerWrap clientPeer = new ClientPeerWrap(this);
                clientPeer.PerformWebSocketHandshake(mSocket);
                if (clientPeer.GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
                {
                    mClientList.Add(clientPeer);
                    PrintAddClientMsg(clientPeer);
                }
                else
                {
                    clientPeer.Reset();
                }
                return true;
            }
            return false;
        }

        private void PrintAddClientMsg(ClientPeerWrap clientPeer)
        {
#if DEBUG
            var mRemoteEndPoint = clientPeer.GetIPEndPoint();
            if (mRemoteEndPoint != null)
            {
                NetLog.Log($"WebSocket 增加客户端: {mRemoteEndPoint}, 客户端总数: {mClientList.Count}");
            }
            else
            {
                NetLog.Log($"WebSocket 增加客户端, 客户端总数: {mClientList.Count}");
            }
#endif
        }

        private void PrintRemoveClientMsg(ClientPeerWrap clientPeer)
        {
#if DEBUG
            var mRemoteEndPoint = clientPeer.GetIPEndPoint();
            if (mRemoteEndPoint != null)
            {
                NetLog.Log($"WebSocket 移除客户端: {mRemoteEndPoint}, 客户端总数: {mClientList.Count}");
            }
            else
            {
                NetLog.Log($"WebSocket 移除客户端, 客户端总数: {mClientList.Count}");
            }
#endif
        }
    }
}
