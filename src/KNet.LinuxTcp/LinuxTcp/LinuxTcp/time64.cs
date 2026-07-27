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
namespace KNet.LinuxTcp.Common
{
    internal class timespec64
    {
        public long tv_sec;            /* seconds */
        public long tv_nsec;       /* nanoseconds */
    }

    internal static partial class LinuxTcpFunc
    {
        //public const long MSEC_PER_SEC = 1000;
        //public const long MSEC_PER_USEC = 1000;
        //public const long MSEC_PER_NSEC = 1000000;
        //public const long USEC_PER_MSEC = 1000;
        //public const long NSEC_PER_USEC = 1000; //1 微秒 = 1000 纳秒。
        //public const long NSEC_PER_MSEC = 1000000;//1 豪秒 = 1000000 纳秒。
        //public const long USEC_PER_SEC = 1000000;
        //public const long NSEC_PER_SEC = 1000000000;//1秒 = 1 000 000 000 纳秒。
    }
}
