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
using System.Runtime.InteropServices;
namespace KNet.Platform
{
    public static unsafe partial class Interop
    {
#if NET7_0_OR_GREATER
        public static unsafe partial class Kernel32
        {
            [LibraryImport(Libraries.Kernel32)]
            public static partial IntPtr HeapCreate(uint flOptions, int dwInitialSize, int dwMaximumSize);
            [LibraryImport(Libraries.Kernel32)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool HeapDestroy(IntPtr hHeap);
            [LibraryImport(Libraries.Kernel32)]
            public static partial void* HeapAlloc(IntPtr hHeap, uint dwFlags, int dwBytes);
            [LibraryImport(Libraries.Kernel32)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool HeapFree(IntPtr hHeap, uint dwFlags, void* lpMem);
        }
#else
        public static unsafe partial class Kernel32
        {
            [DllImport(Libraries.Kernel32)]
            public static extern IntPtr HeapCreate(uint flOptions, int dwInitialSize, int dwMaximumSize);
            [DllImport(Libraries.Kernel32)]
            public static extern bool HeapDestroy(IntPtr hHeap);
            [DllImport(Libraries.Kernel32)]
            public static extern void* HeapAlloc(IntPtr hHeap, uint dwFlags, int dwBytes);
            [DllImport(Libraries.Kernel32)]
            public static extern bool HeapFree(IntPtr hHeap, uint dwFlags, void* lpMem);
        }
#endif
    }
}
