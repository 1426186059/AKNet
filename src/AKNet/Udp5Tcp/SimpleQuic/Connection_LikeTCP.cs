/************************************Copyright*****************************************
 *  Project    : AKNet
 *  Web        : https://github.com/825126369/AKNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/07/02 04:26:51
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using AKNet.Common;

namespace AKNet.Udp5Tcp.Common
{
    internal partial class Connection
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
			if (mConnectionType == E_CONNECTION_TYPE.Client)
			{
				this.OnConnectReset();
				NetLog.Log("Client: Udp 正在连接服务器: " + RemoteEndPoint);
				SendInnerNetData(UdpNetCommand.COMMAND_CONNECT);
			}
		}

		public void SendDisConnect()
		{
			if (mConnectionType == E_CONNECTION_TYPE.Client)
			{
				this.OnDisConnectReset();
				NetLog.Log("Client: Udp 正在 断开服务器: " + RemoteEndPoint);
				SendInnerNetData(UdpNetCommand.COMMAND_DISCONNECT);
			}
		}

		public void ReceiveConnect()
		{
			if (!m_Connected)
			{
				m_Connected = true;
                this.OnConnectReset();
                _connectedTcs.TrySetResult();
                if (mConnectionType == E_CONNECTION_TYPE.Server)
				{
					this.SendInnerNetData(UdpNetCommand.COMMAND_CONNECT);
				}
			}
		}

		public void ReceiveDisConnect()
		{
			if (m_Connected)
			{
                m_Connected = false;
                this.OnDisConnectReset();
                _disConnectedTcs.TrySetResult();
                if (mConnectionType == E_CONNECTION_TYPE.Server)
				{
                    SendInnerNetData(UdpNetCommand.COMMAND_DISCONNECT);
                }
            }
		}

	}

}