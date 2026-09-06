/************************************Copyright*****************************************
 *  Project    : KNet
 *  Web        : https://github.com/1426186059/KNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/09/06 00:00:00
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using KNet.Common;
using System.Net.Sockets;

namespace KNet.WebSocket.Server
{
    public partial class NetServerMain : NetServerInterface
    {
        // ClientPeer 管理器：遍历活跃连接、心跳超时检测与移除。
        // 连接已在 AcceptLoopAsync 中完成 HTTP 升级（系统 HttpListener 已返回 WebSocket），
        // 这里只负责把已建立的 ClientPeerWrap 从队列移入活跃列表，并驱动每帧更新。
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

        private bool MultiThreadingHandleConnectedSocket(ClientPeerWrap mClientPeer)
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
                    mConnectSocketQueue.Enqueue(mClientPeer);
                }
                return true;
            }
        }

        private bool CreateClientPeer()
        {
            ClientPeerWrap mClientPeer = null;
            lock (mConnectSocketQueue)
            {
                mConnectSocketQueue.TryDequeue(out mClientPeer);
            }

            if (mClientPeer != null)
            {
                mClientList.Add(mClientPeer);
                PrintAddClientMsg(mClientPeer);
                return true;
            }

            return false;
        }

        private void PrintAddClientMsg(ClientPeerWrap mClientPeer)
        {
#if DEBUG
            var mRemoteEndPoint = mClientPeer.GetIPEndPoint();
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

        private void PrintRemoveClientMsg(ClientPeerWrap mClientPeer)
        {
#if DEBUG
            var mRemoteEndPoint = mClientPeer.GetIPEndPoint();
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
