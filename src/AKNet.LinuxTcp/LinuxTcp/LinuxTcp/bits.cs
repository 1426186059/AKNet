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
using System.Runtime.CompilerServices;

namespace AKNet.LinuxTcp.Common
{
    internal static partial class LinuxTcpFunc
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong BIT(int nr)
        {
            return 1UL << nr;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool BoolOk(long nr)
        {
            return nr != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool BoolOk(ulong nr)
        {
            return nr != 0;
        }
    }
}
