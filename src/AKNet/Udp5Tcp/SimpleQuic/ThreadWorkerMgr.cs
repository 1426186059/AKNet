/************************************Copyright*****************************************
 *  Project    : AKNet
 *  Web        : https://github.com/825126369/AKNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/07/02 04:26:51
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using AKNet.Common;
using System;
using System.Collections.Generic;

namespace AKNet.Udp5Tcp.Common
{
    internal static class ThreadWorkerMgr
    {
        private static readonly List<ThreadWorker> mThreadWorkerList = new List<ThreadWorker>();
        private static bool bInit = false;

        public static void Init()
        {
            if (bInit) return;
            bInit = true;
            
            int nThreadCount = Environment.ProcessorCount;
            for (int i = 0; i < nThreadCount; i++)
            {
                var mThreadWorker = new ThreadWorker();
                mThreadWorkerList.Add(mThreadWorker);
            }
        }

        public static ThreadWorker GetMainThreadWorker()
        {
            return mThreadWorkerList[0];
        }

        public static ThreadWorker GetThreadWorker(int i)
        {
            return mThreadWorkerList[i];
        }

        public static ThreadWorker GetRandomThreadWorker()
        {
            int nRandomIndex = RandomTool.RandomArrayIndex(0, mThreadWorkerList.Count);
            return mThreadWorkerList[nRandomIndex];
        }

        public static List<ThreadWorker> GetRandomThreadWorkerList(int nSocketCount)
        {
            List<ThreadWorker> mFinalList = new List<ThreadWorker>();

            List<int> mIndexList = new List<int>();
            for(int i = 0; i < mThreadWorkerList.Count; i++)
            {
                mIndexList.Add(i);
            }
            
            while (mFinalList.Count < nSocketCount)
            {
                int nRandomIndex = RandomTool.RandomArrayIndex(0, mIndexList.Count);
                ThreadWorker mThreadWorker = mThreadWorkerList[nRandomIndex];
                mIndexList.RemoveAt(nRandomIndex);
                mFinalList.Add(mThreadWorker);
            }

            NetLog.Assert(mFinalList.Count == nSocketCount);
            return mFinalList;
        }
    }
}









