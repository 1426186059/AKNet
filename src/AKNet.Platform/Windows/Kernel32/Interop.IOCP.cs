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
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace AKNet.Platform
{
    public static unsafe partial class Interop
    {
        public static unsafe partial class Kernel32
        {
#if NET7_0_OR_GREATER
            [LibraryImport(Libraries.Kernel32, SetLastError = true)]
            internal static partial IntPtr CreateIoCompletionPort(IntPtr FileHandle, IntPtr ExistingCompletionPort, IntPtr CompletionKey, int NumberOfConcurrentThreads);

            [LibraryImport(Libraries.Kernel32, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static partial bool PostQueuedCompletionStatus(IntPtr CompletionPort, uint dwNumberOfBytesTransferred, IntPtr CompletionKey, OVERLAPPED* lpOverlapped);

            [LibraryImport(Libraries.Kernel32, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static partial bool GetQueuedCompletionStatus(IntPtr CompletionPort, out uint lpNumberOfBytesTransferred, out IntPtr CompletionKey, OVERLAPPED* lpOverlapped, int dwMilliseconds);

            [LibraryImport(Libraries.Kernel32, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static unsafe partial bool GetQueuedCompletionStatusEx(
                IntPtr CompletionPort,
                OVERLAPPED_ENTRY* lpCompletionPortEntries,
                int ulCount,
                out int ulNumEntriesRemoved,
                int dwMilliseconds,
                [MarshalAs(UnmanagedType.Bool)] bool fAlertable);
#else
            [DllImport(Libraries.Kernel32, SetLastError = true)]
            internal static extern IntPtr CreateIoCompletionPort(IntPtr FileHandle, IntPtr ExistingCompletionPort, IntPtr CompletionKey, int NumberOfConcurrentThreads);

            [DllImport(Libraries.Kernel32, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool PostQueuedCompletionStatus(IntPtr CompletionPort, uint dwNumberOfBytesTransferred, IntPtr CompletionKey, OVERLAPPED* lpOverlapped);

            [DllImport(Libraries.Kernel32, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetQueuedCompletionStatus(IntPtr CompletionPort, out uint lpNumberOfBytesTransferred, out IntPtr CompletionKey, OVERLAPPED* lpOverlapped, int dwMilliseconds);

            [DllImport(Libraries.Kernel32, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static unsafe extern bool GetQueuedCompletionStatusEx(
                IntPtr CompletionPort,
                OVERLAPPED_ENTRY* lpCompletionPortEntries,
                int ulCount,
                out int ulNumEntriesRemoved,
                int dwMilliseconds,
                [MarshalAs(UnmanagedType.Bool)] bool fAlertable);
#endif

        }
    }
}
