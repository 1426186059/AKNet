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

namespace MSQuic2
{
    internal static partial class MSQuicFunc
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CxPlatLockAcquire(object Lock)
        {
            Monitor.Enter(Lock);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CxPlatLockRelease(object Lock)
        {
            Monitor.Exit(Lock);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CxPlatDispatchLockAcquire(object Lock)
        {
            Monitor.Enter(Lock);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CxPlatDispatchLockRelease(object Lock)
        {
            Monitor.Exit(Lock);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CxPlatDispatchRwLockAcquireShared(ReaderWriterLockSlim mLock)
        {
            mLock.EnterReadLock();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CxPlatDispatchRwLockReleaseShared(ReaderWriterLockSlim mLock)
        {
            mLock.ExitReadLock();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CxPlatDispatchRwLockAcquireExclusive(ReaderWriterLockSlim mLock)
        {
            mLock.EnterWriteLock();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CxPlatDispatchRwLockReleaseExclusive(ReaderWriterLockSlim mLock)
        {
            mLock.ExitWriteLock();
        }

        //原子地获取一个布尔值，并将其设置为 false。
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int InterlockedFetchAndClearBoolean(ref int Target)
        {
            return InterlockedEx.And(ref Target, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int InterlockedFetchAndSetBoolean(ref int Target)
        {
            return InterlockedEx.Or(ref Target, 1);
        }
    }
}
