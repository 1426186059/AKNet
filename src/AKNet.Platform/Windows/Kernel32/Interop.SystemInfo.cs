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
using System.Runtime.InteropServices;
namespace AKNet.Platform
{
    public static unsafe partial class Interop
    {
#if NET7_0_OR_GREATER
        public static unsafe partial class Kernel32
        {
            [LibraryImport("kernel32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool GlobalMemoryStatusEx(MEMORYSTATUSEX* lpBuffer);
            [LibraryImport("kernel32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static partial bool GetSystemTimeAdjustment(out int lpTimeAdjustment, 
                out int lpTimeIncrement, 
                [MarshalAs(UnmanagedType.Bool)] out bool lpTimeAdjustmentDisabled);
        }
#else
        public static unsafe partial class Kernel32
        {
            [DllImport("kernel32.dll")]
            public static extern bool GlobalMemoryStatusEx(MEMORYSTATUSEX* lpBuffer);
            [DllImport("kernel32.dll")]
            public static extern bool GetSystemTimeAdjustment(out int lpTimeAdjustment, out int lpTimeIncrement, out bool lpTimeAdjustmentDisabled);
        }
#endif
    }
}
