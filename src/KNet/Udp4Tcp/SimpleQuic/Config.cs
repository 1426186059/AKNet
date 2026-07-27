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

namespace KNet.Udp4Tcp.Common
{
    internal static class Config
	{
        public static readonly int nSocketCount = 1;
        public const bool bUseSocketAsyncEventArgsTwoComplete = true;
        public const bool bUseSingleSendArgs = false;

        public const int nUdpPackageFixedSize = CommonUdpLayerConfig.nUdpPackageFixedSize;
		public const int nUdpPackageFixedHeadSize = 12;
        public const int nUdpPackageFixedBodySize = nUdpPackageFixedSize - nUdpPackageFixedHeadSize;
        public const int nMaxDataLength = CommonTcpLayerConfig.nDataMaxLength;

        public const uint nUdpMinOrderId = UdpNetCommand.COMMAND_MAX + 1;
        public const uint nUdpMaxOrderId = uint.MaxValue;
	}
}
