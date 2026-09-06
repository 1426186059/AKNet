// ClientPeer 管理器：活跃连接登记/移除、心跳超时检测、状态变更派发。
using Fleck;
using KNet.Common;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace KNet.WebSocket.Server
{
    public partial class NetServerMain : NetServerInterface
    {
        // 由定时器驱动：发送心跳 + 检测接收心跳超时（超时者从列表移除并关闭）
        // 优化（对齐 TcpListener 思路）：锁只用于“跨线程交接 + 列表结构变更”，绝不包住逐连接的工作。
        // TcpListener 把跨线程交接收敛到 mConnectSocketQueue（仅它加锁），客户端列表只在主线程改动故无需锁遍历；
        // Fleck/HttpListener 的 OnOpen/OnClose 在 Fleck 线程上直接改动列表，列表确为多线程写入，但仍应把锁收敛到
        // 最短范围——peer.Update 内含同步 mSocket.Send（可能较慢），若持锁遍历会让 OnOpen/OnClose 回调排队，阻塞新连接接入。
        public void Update(double elapsed)
        {
            if (elapsed >= 0.3) NetLog.LogWarning("帧 时间 太长: " + elapsed);
            
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

        public bool MultiThreadingHandleConnectedSocket(ClientPeer mSocket)
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
            ClientPeer mSocket = null;
            lock (mConnectSocketQueue)
            {
                mConnectSocketQueue.TryDequeue(out mSocket);
            }
            if (mSocket != null)
            {
                ClientPeerWrap clientPeer = new ClientPeerWrap(mSocket, this);
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
