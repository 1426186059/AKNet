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
using System.Runtime.CompilerServices;
using System.Threading;

namespace AKNet.Udp4Tcp.Common
{
    internal enum ConnectionAsyncOperation
    {
        None = 0,
        Accept,
        Connect,
        Disconnect,
        Receive,
        Send,
    }

    internal enum ConnectionError
    {
        Success = 1,
        Error = 2,
    }

    internal enum E_LOGIC_RESULT
    {
        Success = 0,
        Error = 1,
    }

    internal enum E_CONNECTION_TYPE
    {
        Client,
        Server,
    }

    internal static partial class SimpleQuicFunc
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool FAILED(E_LOGIC_RESULT Status)
        {
            return Status == E_LOGIC_RESULT.Error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SUCCESSED(E_LOGIC_RESULT Status)
        {
            return Status == E_LOGIC_RESULT.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThreadCheck(Connection mConnection)
        {
            ThreadCheck(mConnection.mLogicWorker);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThreadCheck(LogicWorker mLogicWorker)
        {
#if DEBUG
            int nThreadId = Thread.CurrentThread.ManagedThreadId;
            if (nThreadId != mLogicWorker.mThreadWorker.ThreadID)
            {
                NetLog.LogError($"ThreadCheck: {mLogicWorker.mThreadWorker.ThreadID}, {nThreadId}");
            }
#endif
        }
    }
}
