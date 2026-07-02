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
#if USE_MSQUIC_2 
using MSQuic2;
#else
using MSQuic1;
#endif
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TestNetClient")]
namespace AKNet.Common
{
    internal static class udp_statistic
    {
        public static void PrintInfo()
        {
            NetLog.Log("-----------");
            NetLog.Log("Quic UDP 所有分区总共统计信息: ");
            MSQuicFunc.QuicPerfCounterSnapShot(0);
            MSQuicFunc.udp_statistic_printInfo();
        }
    }
}
