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
namespace AKNet.LinuxTcp.Common
{
    internal static partial class LinuxTcpFunc
    {
        public static bool time_after(long a, long b)
        {
            return a > b;
        }

        public static bool time_before(long a, long b)
        {
            return time_after(b, a);
        }

        public static bool time_after_eq(long a, long b)
        {
            return a >= b;
        }

        public static bool time_before_eq(long a, long b)
        {
            return time_after_eq(b, a);
        }
    }
}
