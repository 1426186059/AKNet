/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库
*        Author:许珂
*        StartTime:2024/11/01 00:00:00
*        ModifyTime:2026/2/1 20:27:10
*        Copyright:MIT软件许可证
************************************Copyright*****************************************/
using AKNet.Common;
using AKNet.LinuxTcp.Common;
using System;

namespace AKNet.LinuxTcp.Server
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
			Reset();
			mUdpCheckPool.InitConnect();
			mUdpCheckPool.FinishConnect(skb);

            fReceiveHeartBeatTime = 0.0;
			fMySendHeartBeatCdTime = 0.0;
			SetSocketState(SOCKET_PEER_STATE.CONNECTED);
			SendInnerNetData(UdpNetCommand.COMMAND_CONNECT);
		}

		public void ReceiveDisConnect()
		{
			Reset();
			fMySendHeartBeatCdTime = 0.0;
			fReceiveHeartBeatTime = 0.0;
			SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
			SendInnerNetData(UdpNetCommand.COMMAND_DISCONNECT);
		}
	}
}
