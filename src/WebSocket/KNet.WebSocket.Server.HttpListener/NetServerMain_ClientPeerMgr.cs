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

            for (int i = 0; i < mClientList.Count;)
            {
                ClientPeerWrap mClientPeer = mClientList[i];
                if (mClientPeer.GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
                {
                    mClientPeer.Update(elapsed);
                    ++i;
                }
                else
                {
                    //移除元素的两种方法
                    //1: mClientList.RemoveAt(i); 这样移除，会移动很多元素，效率低下

                    //2:这是优化的移除方法，交换元素即可。 可推广到其他工程里
                    var mRemove = mClientPeer;

                    int nLastIndex = mClientList.Count - 1;
                    mClientList[i] = mClientList[nLastIndex];
                    mClientList.RemoveAt(nLastIndex);

                    PrintRemoveClientMsg(mRemove);
                    mRemove.Reset();
                }
            }
        }

        private bool MultiThreadingHandleConnectedSocket(FakeSocket mSocket)
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
            FakeSocket mSocket;
            lock (mConnectSocketQueue)
            {
                mConnectSocketQueue.TryDequeue(out mSocket);
            }

            if (mSocket.mContext != null)
            {
                ClientPeerWrap clientPeer = new ClientPeerWrap(this);
                clientPeer.HandleConnectedSocket(mSocket);
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
