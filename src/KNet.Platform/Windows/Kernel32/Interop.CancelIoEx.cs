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
        public static unsafe partial class Kernel32
        {
#if NET7_0_OR_GREATER
            [LibraryImport(Libraries.Kernel32, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static unsafe partial bool CancelIoEx(SafeHandle handle, OVERLAPPED* lpOverlapped);

            [LibraryImport(Libraries.Kernel32, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static unsafe partial bool CancelIoEx(IntPtr handle, OVERLAPPED* lpOverlapped);
#else
            [DllImport(Libraries.Kernel32, SetLastError = true)]
            internal static unsafe extern bool CancelIoEx(SafeHandle handle, OVERLAPPED* lpOverlapped);

            [DllImport(Libraries.Kernel32, SetLastError = true)]
            internal static unsafe extern bool CancelIoEx(IntPtr handle, OVERLAPPED* lpOverlapped);
#endif
        }
    }
}
