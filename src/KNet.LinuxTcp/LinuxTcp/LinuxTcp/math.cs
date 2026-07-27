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
using System;

namespace KNet.LinuxTcp.Common
{
    internal static partial class LinuxTcpFunc
    {
        static int rounddown(int __x, int y)
        {
            return __x - (__x % (y));
        }

        static int roundup_pow_of_two(int n)
        {
            if (n == 0) return 1;
            int result = 1;
            while (result < n) result <<= 1;
            return result;
        }

        static int min3(int x, int y, int z)
        {
            var t = Math.Min(x, y);
            t = Math.Min(z, t);
            return t;
        }

        static long DIV_ROUND_UP(long x, long y)
        {
            if (x % y == 0)
            {
                return x / y;
            }
            else
            {
                return x / y + 1;
            }
        }
    }
}
