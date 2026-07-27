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
using KNet.Udp3Tcp.Common;
using System.Collections.Generic;

namespace KNet.Udp3Tcp.Server
{
    internal class FakeSocketPool
    {
        readonly Stack<FakeSocket> mObjectPool = new Stack<FakeSocket>();
        NetServerMain mUdpServer = null;
        private int nMaxCapacity = 0;
        private FakeSocket GenerateObject()
        {
            FakeSocket clientPeer = new FakeSocket(this.mUdpServer);
            return clientPeer;
        }

        public FakeSocketPool(NetServerMain mUdpServer, int initCapacity = 0, int nMaxCapacity = 0)
        {
            this.mUdpServer = mUdpServer;
            SetMaxCapacity(nMaxCapacity);
            for (int i = 0; i < initCapacity; i++)
            {
                FakeSocket clientPeer = GenerateObject();
                mObjectPool.Push(clientPeer);
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

        public FakeSocket Pop()
        {
            FakeSocket t = null;
            lock (mObjectPool)
            {
                mObjectPool.TryPop(out t);
            }

            if (t == null)
            {
                t = GenerateObject();
            }

            return t;
        }

        public void recycle(FakeSocket t)
        {
#if DEBUG
            NetLog.Assert(!mObjectPool.Contains(t));
#endif
            t.Reset();
            bool bRecycle = nMaxCapacity <= 0 || mObjectPool.Count < nMaxCapacity;
            if (bRecycle)
            {
                lock (mObjectPool)
                {
                    mObjectPool.Push(t);
                }
            }
        }
    }
}
