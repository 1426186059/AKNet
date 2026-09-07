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
using System.Net.Sockets;

namespace KNet.Tcp2.Server
{
    internal partial class NetServerMain
    {
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
                    var mRemove = mClientPeer;

                    int nLastIndex = mClientList.Count - 1;
                    mClientList[i] = mClientList[nLastIndex];
                    mClientList.RemoveAt(nLastIndex);

                    PrintRemoveClientMsg(mRemove);
                    mRemove.Reset();
                }
            }
        }

        FrameUpdateFunc mFrameUpdateFunc = null;
        public void Update()
        {
            if (mFrameUpdateFunc == null)
            {
                mFrameUpdateFunc = new FrameUpdateFunc();
            }
            mFrameUpdateFunc.Update(Update);
        }

        public bool MultiThreadingHandleConnectedTcpClient(TcpClient client)
        {
            int nNowConnectCount = mClientList.Count + mConnectTcpClientQueue.Count;
            if (nNowConnectCount >= this.mConfigInstance.MaxPlayerCount)
            {
#if DEBUG
                NetLog.Log($"服务器爆满, 客户端总数: {nNowConnectCount}");
#endif
                return false;
            }
            else
            {
                lock (mConnectTcpClientQueue)
                {
                    mConnectTcpClientQueue.Enqueue(client);
                }
                return true;
            }
        }

        private bool CreateClientPeer()
        {
            TcpClient client = null;
            lock (mConnectTcpClientQueue)
            {
                mConnectTcpClientQueue.TryDequeue(out client);
            }
            if (client != null)
            {
                ClientPeerWrap clientPeer = new ClientPeerWrap(this);
                clientPeer.HandleConnectedTcpClient(client);
                mClientList.Add(clientPeer);
                PrintAddClientMsg(clientPeer);
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
                NetLog.Log($"增加客户端: {mRemoteEndPoint}, 客户端总数: {mClientList.Count}");
            }
            else
            {
                NetLog.Log($"增加客户端, 客户端总数: {mClientList.Count}");
            }
#endif
        }

        private void PrintRemoveClientMsg(ClientPeerWrap clientPeer)
        {
#if DEBUG
            var mRemoteEndPoint = clientPeer.GetIPEndPoint();
            if (mRemoteEndPoint != null)
            {
                NetLog.Log($"移除客户端: {mRemoteEndPoint}, 客户端总数: {mClientList.Count}");
            }
            else
            {
                NetLog.Log($"移除客户端, 客户端总数: {mClientList.Count}");
            }
#endif
        }
    }
}
