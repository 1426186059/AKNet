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
using System.Net;
using System.Net.Sockets;

namespace KNet.Udp5Tcp.Common
{
    internal partial class Listener
    {
        public void MultiThreadingReceiveNetPackage(SocketAsyncEventArgs e)
        { 
            SocketItem mSocketItem = e.UserToken as SocketItem;
            IPEndPoint nPeerId = (IPEndPoint)e.RemoteEndPoint;
            Connection mConnectionPeer = null;

            //1: 这里存在一个问题：如果使用多个Socket 同时处理包的话，这里会产生竞争。由于是多个线程竞争，会造成性能瓶颈。
            //2: 暂时这个 SimpleQuic 只考虑1个Socket. 所以这个性能瓶颈暂时不处理了。
            lock (mConnectionPeerDic)
            {
                mConnectionPeerDic.TryGetValue(nPeerId, out mConnectionPeer);
            }

            if (mConnectionPeer == null)
            {
                mConnectionPeer = new Connection();
                mConnectionPeer.RemoteEndPoint = nPeerId;
                mSocketItem.mLogicWorker.AddConnection(mConnectionPeer);
                mConnectionPeer.Init(E_CONNECTION_TYPE.Server);
                mConnectionPeer.OwnerListener = this;
                lock (mConnectionPeerDic)
                {
                    mConnectionPeerDic.Add(nPeerId, mConnectionPeer);
                }

                HandleNewConntion(mConnectionPeer);
                PrintAddFakeSocketMsg(mConnectionPeer);
            }

            if (mConnectionPeer != null)
            {
                mConnectionPeer.WorkerThreadReceiveNetPackage(e);
            }
        }

        public void RemoveFakeSocket(Connection mConnectionPeer)
        {
            SimpleQuicFunc.ThreadCheck(mConnectionPeer);
            var peerId = mConnectionPeer.RemoteEndPoint;
            lock (mConnectionPeerDic)
            {
                mConnectionPeerDic.Remove(peerId);
            }
            
            PrintRemoveFakeSocketMsg(mConnectionPeer);
        }

        private void PrintAddFakeSocketMsg(Connection mSocket)
        {
#if DEBUG
            var mRemoteEndPoint = mSocket.RemoteEndPoint;
            if (mRemoteEndPoint != null)
            {
                NetLog.Log($"增加FakeSocket: {mRemoteEndPoint}, FakeSocket总数: {mConnectionPeerDic.Count}");
            }
            else
            {
                NetLog.Log($"增加FakeSocket, FakeSocket总数: {mConnectionPeerDic.Count}");
            }
#endif
        }

        private void PrintRemoveFakeSocketMsg(Connection mSocket)
        {
#if DEBUG
            var mRemoteEndPoint = mSocket.RemoteEndPoint;
            if (mRemoteEndPoint != null)
            {
                NetLog.Log($"移除FakeSocket: {mRemoteEndPoint}, FakeSocket总数: {mConnectionPeerDic.Count}");
            }
            else
            {
                NetLog.Log($"移除FakeSocket, FakeSocket总数: {mConnectionPeerDic.Count}");
            }
#endif
        }
    }
}