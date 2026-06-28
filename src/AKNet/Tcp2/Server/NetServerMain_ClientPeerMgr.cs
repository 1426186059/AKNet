/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库 - TcpListener 实现
*        Author:许珂
*        CreateTime:2026/6/28
*        Copyright:MIT软件许可证
************************************Copyright*****************************************/
using AKNet.Common;
using System.Net.Sockets;

namespace AKNet.Tcp2.Server
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
