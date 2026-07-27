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
        public static unsafe partial class Winsock
        {
#if NET7_0_OR_GREATER
            [LibraryImport(Interop.Libraries.Ws2_32, SetLastError = true)]
            private static unsafe partial int WSASendTo(
                IntPtr socketHandle,
                WSABUF* buffers,
                int bufferCount,
                out int bytesTransferred,
                uint socketFlags,
                ReadOnlySpan<byte> socketAddress,
                int socketAddressSize,
                Overlapped* overlapped,
                IntPtr completionRoutine);
#else
            [DllImport(Interop.Libraries.Ws2_32, SetLastError = true)]
            private static unsafe extern int WSASendTo(
                IntPtr socketHandle,
                WSABUF* buffers,
                int bufferCount,
                out int bytesTransferred,
                uint socketFlags,
                ReadOnlySpan<byte> socketAddress,
                int socketAddressSize,
                Overlapped* overlapped,
                IntPtr completionRoutine);
#endif
        }
    }
}
