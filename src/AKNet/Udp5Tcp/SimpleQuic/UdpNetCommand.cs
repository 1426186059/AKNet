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
namespace AKNet.Udp5Tcp.Common
{
    internal static class UdpNetCommand
	{
		public const byte COMMAND_PACKAGE_CHECK_SURE_ORDERID = 1;
		public const byte COMMAND_HEARTBEAT = 2;
		public const byte COMMAND_CONNECT = 3;
		public const byte COMMAND_DISCONNECT = 4;
        public const byte COMMAND_MAX = 10;

		public static bool orInnerCommand(ushort id)
		{
			return id >= 1 && id <= COMMAND_MAX;
		}
	}
}
