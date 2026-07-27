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
using KNet.Udp1Tcp.Common;

namespace KNet.Udp1Tcp.Client
{
    internal partial class NetClientMain
    {
		private void SendHeartBeat()
		{
			SendInnerNetData(UdpNetCommand.COMMAND_HEARTBEAT);
		}

        public void ResetSendHeartBeatCdTime()
        {
            fMySendHeartBeatCdTime = 0.0;
        }

        public void ReceiveHeartBeat()
		{
			fReceiveHeartBeatTime = 0.0;
        }

		public void SendConnect()
		{
            Reset();
			SetSocketState(SOCKET_PEER_STATE.CONNECTING);
			NetLog.Log($"{NetType.Udp1Tcp.ToString()} 客户端 正在连接服务器: {remoteEndPoint}");
			SendInnerNetData(UdpNetCommand.COMMAND_CONNECT);
		}

		public void SendDisConnect()
		{
            Reset();
			SetSocketState(SOCKET_PEER_STATE.DISCONNECTING);
			NetLog.Log($"{NetType.Udp1Tcp.ToString()} 客户端 正在 断开服务器: {remoteEndPoint}");
			SendInnerNetData(UdpNetCommand.COMMAND_DISCONNECT);
		}

		public void ReceiveConnect()
		{
			if (GetSocketState() != SOCKET_PEER_STATE.CONNECTED)
			{
				this.Reset();
				SetSocketState(SOCKET_PEER_STATE.CONNECTED);
				NetLog.Log($"{NetType.Udp1Tcp.ToString()} 客户端 连接服务器 成功");
			}
		}

		public void ReceiveDisConnect()
		{
			if (GetSocketState() != SOCKET_PEER_STATE.DISCONNECTED)
			{
                Reset();
				SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
				NetLog.Log($"{NetType.Udp1Tcp.ToString()} 客户端 断开服务器 成功!");
			}
		}
    }

}