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
using AKNet.Udp1Tcp.Common;

namespace AKNet.Udp1Tcp.Client
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