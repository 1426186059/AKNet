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
    internal static class UdpNetCommand
	{
		public const byte COMMAND_HEARTBEAT = 1;
		public const byte COMMAND_CONNECT = 2;
		public const byte COMMAND_DISCONNECT = 3;
        public const byte COMMAND_MAX = 10;

		public static bool orInnerCommand(ushort id)
		{
			return id >= 1 && id <= COMMAND_MAX;
		}
	}
}
