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
using System.Collections.Generic;

namespace KNet.Udp5Tcp.Server
{
    internal class ClientPeerPool
    {
        readonly Stack<ClientPeer> mObjectPool = new Stack<ClientPeer>();
        NetServerMain mServer = null;
        private int nMaxCapacity = 0;
        private ClientPeer GenerateObject()
        {
            ClientPeer clientPeer = new ClientPeer(this.mServer);
            return clientPeer;
        }

        public ClientPeerPool(NetServerMain mTcpServer, int initCapacity = 0, int nMaxCapacity = 0)
        {
            this.mServer = mTcpServer;
            SetMaxCapacity(nMaxCapacity);
            for (int i = 0; i < initCapacity; i++)
            {
                mObjectPool.Push(GenerateObject());
            }
        }

        public void SetMaxCapacity(int nCapacity)
        {
            this.nMaxCapacity = nCapacity;
        }

        public int Count()
        {
            return mObjectPool.Count;
        }

        public ClientPeer Pop()
        {
            MainThreadCheck.Check();

            ClientPeer t = null;
            if (!mObjectPool.TryPop(out t))
            {
                t = GenerateObject();
            }
            return t;
        }

        public void recycle(ClientPeer t)
        {
            MainThreadCheck.Check();
#if DEBUG
            NetLog.Assert(!mObjectPool.Contains(t));
#endif
            t.Reset();
            //防止 内存一直增加，合理的GC
            bool bRecycle = nMaxCapacity <= 0 || mObjectPool.Count < nMaxCapacity;
            if (bRecycle)
            {
                mObjectPool.Push(t);
            }
            else
            {
                t.Dispose();
            }
        }
    }
}
