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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
[assembly: InternalsVisibleTo("AKNet")]
[assembly: InternalsVisibleTo("AKNet.MSQuic")]
[assembly: InternalsVisibleTo("AKNet.LinuxTcp")]
[assembly: InternalsVisibleTo("AKNet.WebSocket")]
namespace AKNet.Common
{
    internal static class MainThreadCheck
    {
        static readonly int nMainThreadId = Thread.CurrentThread.ManagedThreadId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool orInMainThread()
        {
            return Thread.CurrentThread.ManagedThreadId == nMainThreadId;
        }

        [Conditional("DEBUG")]
        public static void Check()
        {
            int nThreadId = Thread.CurrentThread.ManagedThreadId;
            if (nThreadId != nMainThreadId)
            {
                NetLog.LogError($"MainThreadCheck: {nMainThreadId}, {nThreadId}");
            }
        }
    }

    internal class ThreadOnlyOneCheck
    {
        int nThreadCount = 0;
        public void Enter(string tag = "")
        {
            NetLog.Assert(nThreadCount == 0, $"{nThreadCount}个线程同时访问同一个代码块: {tag}");
            nThreadCount++;
        }

        public void Exit()
        {
            nThreadCount--;
        }
    }
}
