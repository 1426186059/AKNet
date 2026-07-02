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
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

[assembly: InternalsVisibleTo("AKNet.MSTest")]
namespace AKNet.Common
{
    //这个类需要测试，现在不能保证操作原子性的，也就是100个线程都执行 Increment（）操作：不能保证结果等于100
    public unsafe static class InterlockedEx
    {
        //返回值：递增后的值。
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Increment(ref ulong location)
        {
            fixed (void* ptr = &location)
            {
                return (ulong)Interlocked.Increment(ref MemoryMarshal.GetReference(new ReadOnlySpan<long>(ptr, 1)));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int And(ref int location1, int value)
        {
            int current = location1;
            while (true)
            {
                int newValue = current & value;
                int oldValue = Interlocked.CompareExchange(ref location1, newValue, current);
                if (oldValue == current)
                {
                    return oldValue;
                }
                current = oldValue;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Or(ref int location1, int value)
        {
            int current = location1;
            while (true)
            {
                int newValue = current | value;
                int oldValue = Interlocked.CompareExchange(ref location1, newValue, current);
                if (oldValue == current)
                {
                    return oldValue;
                }
                current = oldValue;
            }
        }
    }
}
