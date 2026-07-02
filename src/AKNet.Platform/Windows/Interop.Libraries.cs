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
namespace AKNet.Platform
{
    public static unsafe partial class Interop
    {
        public static class Libraries
        {
            internal const string Kernel32 = "kernel32.dll";
            internal const string Ucrtbase = "ucrtbase.dll";
            internal const string Ws2_32 = "ws2_32.dll";
            internal const string BCrypt = "BCrypt.dll";
            internal const string NtDll = "ntdll.dll";
        }
    }
}
