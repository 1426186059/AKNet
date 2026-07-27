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
    internal partial class LinuxTcpFunc
    {
        //tcp_metrics，默认情况下，不启用哈，所以代码全部注释掉了
        static void tcp_init_metrics(tcp_sock tp)
        {
            if (tp.srtt_us == 0)
            {
                tp.rttvar_us = TCP_TIMEOUT_FALLBACK;
                tp.mdev_us = tp.mdev_max_us = tp.rttvar_us;
                tp.icsk_rto = TCP_TIMEOUT_FALLBACK;
            }
        }

    }

}
