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

namespace KNet.Udp2Tcp.Common
{
	internal static class UdpNetCommand
	{
		public const ushort COMMAND_PACKAGE_CHECK_SURE_ORDERID = 1;
		public const ushort COMMAND_HEARTBEAT = 2;
		public const ushort COMMAND_CONNECT = 3;
		public const ushort COMMAND_DISCONNECT = 4;

        public const ushort COMMAND_MAX = 10;

        public static bool orNeedCheck(ushort id)
		{
			return id > COMMAND_MAX;
		}

		public static bool orInnerCommand(ushort id)
		{
			return id >= 1 && id <= COMMAND_MAX;
		}
	}
}
