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
using KNet.LinuxTcp.Common;
using System;

namespace KNet.LinuxTcp.Server
{
    internal partial class ClientPeer
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

		public void ReceiveConnect(sk_buff skb)
		{
			OnConnectReset();
			mUdpCheckPool.InitConnect();
			mUdpCheckPool.FinishConnect(skb);
			SetSocketState(SOCKET_PEER_STATE.CONNECTED);
			SendInnerNetData(UdpNetCommand.COMMAND_CONNECT);
		}

		public void ReceiveDisConnect()
		{
			OnDisConnectReset();
			SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
			SendInnerNetData(UdpNetCommand.COMMAND_DISCONNECT);
		}
	}
}
