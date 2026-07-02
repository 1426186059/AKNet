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
using System;

namespace AKNet.LinuxTcp.Common
{
    internal static partial class LinuxTcpFunc
    {
        static ushort ipv4_mtu()
        {
	        return (ushort)Math.Min(1500U, (ushort)IPAddressHelper.GetMtu());
        }

        static ushort ipv4_default_advmss(tcp_sock tp)
        {
            ushort advmss = (ushort)Math.Max(ipv4_mtu() - mtu_max_head_length, sock_net(tp).ipv4.ip_rt_min_advmss);
            return (ushort)Math.Min((int)advmss, IPV4_MAX_PMTU - mtu_max_head_length);
        }
    }
}
