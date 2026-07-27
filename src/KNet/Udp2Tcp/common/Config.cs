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
using KNet.Common;

namespace KNet.Udp2Tcp.Common
{
    internal static class Config
	{
        public const bool bUdpCheck = true;
        public const bool bUseSendAsync = true;
        public const ushort nUdpMinOrderId = UdpNetCommand.COMMAND_MAX + 1;
		public const ushort nUdpMaxOrderId = ushort.MaxValue;
		public const int nUdpPackageFixedHeadSize = 8;
        public const int nUdpPackageFixedBodySize = CommonUdpLayerConfig.nUdpPackageFixedSize - nUdpPackageFixedHeadSize;
	}
}
