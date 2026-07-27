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
using static KNet.Platform.Interop.Kernel32;

namespace KNet.Platform
{
    public static unsafe partial class Interop
    {
        public static unsafe partial class Winsock
        {
#if NET7_0_OR_GREATER
            [LibraryImport(Interop.Libraries.Ws2_32, SetLastError = true)]
            public static unsafe partial int WSAIoctl(
                SafeHandle socketHandle,
                uint ioControlCode,
                void* lpvInBuffer,
                int lpvInBufferLen,
                void* lpvOutBuffer,
                int lpvOutBufferLen,
                out int bytesTransferred,
                void* shouldBeNull,
                void* shouldBeNull2);

            [LibraryImport(Interop.Libraries.Ws2_32, EntryPoint = "WSAIoctl", SetLastError = true)]
            public static partial int WSAIoctl_Blocking(
                SafeHandle socketHandle,
                uint ioControlCode,
                byte[]? inBuffer,
                int inBufferSize,
                byte[]? outBuffer,
                int outBufferSize,
                out int bytesTransferred,
                IntPtr overlapped,
                IntPtr completionRoutine);

            [LibraryImport(Interop.Libraries.Ws2_32, SetLastError = true)]
            public static unsafe partial int WSAIoctl(
                IntPtr socketHandle,
                uint ioControlCode,
                void* lpvInBuffer,
                int lpvInBufferLen,
                void* lpvOutBuffer,
                int lpvOutBufferLen,
                out int bytesTransferred,
                void* shouldBeNull,
                void* shouldBeNull2);

#else
            [DllImport(Interop.Libraries.Ws2_32, SetLastError = true)]
            public static unsafe extern int WSAIoctl(
                IntPtr socketHandle,
                uint ioControlCode,
                void* lpvInBuffer,
                int lpvInBufferLen,
                void* lpvOutBuffer,
                int lpvOutBufferLen,
                out int bytesTransferred,
                void* shouldBeNull,
                void* shouldBeNull2);

            [DllImport(Interop.Libraries.Ws2_32, SetLastError = true)]
            public static unsafe extern int WSAIoctl(
                SafeHandle socketHandle,
                uint ioControlCode,
                void* lpvInBuffer,
                int lpvInBufferLen,
                void* lpvOutBuffer,
                int lpvOutBufferLen,
                out int bytesTransferred,
                void* shouldBeNull,
                void* shouldBeNull2);

            [DllImport(Interop.Libraries.Ws2_32, EntryPoint = "WSAIoctl", SetLastError = true)]
            public static extern int WSAIoctl_Blocking(
                SafeHandle socketHandle,
                uint ioControlCode,
                byte[]? inBuffer,
                int inBufferSize,
                byte[]? outBuffer,
                int outBufferSize,
                out int bytesTransferred,
                IntPtr overlapped,
                IntPtr completionRoutine);
#endif
        }
    }
}
