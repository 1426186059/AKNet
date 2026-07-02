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
using System.Runtime.CompilerServices;
using System.Threading;

namespace MSQuic1
{
    internal static partial class MSQuicFunc
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CxPlatEventInitialize(out EventWaitHandle Event, bool ManualReset, bool InitialState)
        {
            if (ManualReset)
            {
                Event = new ManualResetEvent(InitialState);
            }
            else
            {
                Event = new AutoResetEvent(InitialState);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CxPlatEventUninitialize(EventWaitHandle Event)
        {
            Event.Close();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool CxPlatEventSet(EventWaitHandle Event)
        {
             return Event.Set();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool CxPlatEventReset(EventWaitHandle Event)
        {
            return Event.Reset();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool CxPlatEventWaitForever(EventWaitHandle Event)
        {
            return Event.WaitOne();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool CxPlatEventWaitWithTimeout(EventWaitHandle Event, int TimeoutMs)
        {
            return Event.WaitOne(TimeoutMs);
        }

    }
}
