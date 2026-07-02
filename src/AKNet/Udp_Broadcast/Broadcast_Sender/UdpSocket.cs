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
using System;
using System.Net;
using System.Net.Sockets;
using AKNet.Common;

namespace AKNet.Udp.Broadcast.Sender
{
    public class SocketUdp_Basic
	{
		private EndPoint remoteSendBroadCastEndPoint = null;
		private Socket mSocket = null;

		public void InitNet(UInt16 ServerPort)
		{
			mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			IPEndPoint iep = new IPEndPoint(IPAddress.Broadcast, ServerPort);
			remoteSendBroadCastEndPoint = (EndPoint)iep;
			mSocket.EnableBroadcast = true;

			NetLog.Log("初始化 广播发送器 成功");
		}

		public void SendNetStream(byte[] msg, int offset, int count)
		{
			try
			{
				mSocket.SendTo(msg, offset, count, SocketFlags.None, remoteSendBroadCastEndPoint);
			}
			catch { }
		}

		private void CloseNet()
		{
			mSocket.Close();
		}

		public virtual void Dispose()
		{
			this.CloseNet();
			NetLog.Log("--------------- BroadcastSender  Release ----------------");
		}
	}
}









