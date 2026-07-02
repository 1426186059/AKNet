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
using AKNet.Common;

namespace AKNet.LinuxTcp.Common
{
    public static class UdpStatistical
    {
        public static void PrintLog()
        {
            NetLog.Log($"Udp PackageStatistical:");
            TcpMibMgr.PRINT_NET_STATS();
        }
    }
}
