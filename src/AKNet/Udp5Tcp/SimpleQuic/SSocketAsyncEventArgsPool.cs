/************************************Copyright*****************************************
 *  Project    : AKNet
 *  Web        : https://github.com/1426186059/AKNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/07/02 18:05:45
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using AKNet.Common;
using System.Collections.Generic;

namespace AKNet.Udp5Tcp.Common
{
    internal class SSocketAsyncEventArgsPool
    {
        public LogicWorker mLogicWorker;
        private readonly Stack<SSocketAsyncEventArgs> mObjectPool = new Stack<SSocketAsyncEventArgs>();
        private readonly int nMaxCapacity = 0;

        public SSocketAsyncEventArgsPool(LogicWorker mLogicWorker, int initCapacity = 0, int MaxCapacity = 0)
        {
            this.mLogicWorker = mLogicWorker;
            this.nMaxCapacity = MaxCapacity;
            for (int i = 0; i < initCapacity; i++)
            {
                mObjectPool.Push(Alloc());
            }
        }

        private SSocketAsyncEventArgs Alloc()
        {
            return mLogicWorker.mSocketItem.AllocSSocketAsyncEventArgs();
        }

        public int Count()
        {
            return mObjectPool.Count;
        }

        public SSocketAsyncEventArgs Pop()
        {
            SSocketAsyncEventArgs t = null;
            if (!mObjectPool.TryPop(out t))
            {
                t = Alloc();
            }
            return t;
        }

        public void recycle(SSocketAsyncEventArgs t)
        {
#if DEBUG
            NetLog.Assert(!mObjectPool.Contains(t));
#endif
            t.UserToken = null;
            t.RemoteEndPoint = null;
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
