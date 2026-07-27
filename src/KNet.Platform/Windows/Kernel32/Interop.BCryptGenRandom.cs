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
        public static unsafe partial class BCrypt
        {
            internal const int BCRYPT_USE_SYSTEM_PREFERRED_RNG = 0x00000002;

            [LibraryImport(Libraries.BCrypt)]
            public static unsafe partial int BCryptGenRandom(IntPtr hAlgorithm, byte* pbBuffer, int cbBuffer, int dwFlags);
        }
#else
        public static unsafe partial class BCrypt
        {
            [DllImport(Libraries.BCrypt)]
            public static unsafe extern int BCryptGenRandom(IntPtr hAlgorithm, byte* pbBuffer, int cbBuffer, int dwFlags);
        }
#endif
    }
}
