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
using System;
using AKNet.Common;
using AKNet.Udp.Broadcast.Common;


namespace AKNet.Udp.Broadcast.Sender
{
    public class ClientPeer : SocketUdp_Basic
	{
		private SafeObjectPool<NetUdpFixedSizePackage> mNetPackagePool = null;

		public ClientPeer()
		{
			mNetPackagePool = new SafeObjectPool<NetUdpFixedSizePackage>(2);
		}

		public void SendNetData(UInt16 id, byte[] buffer)
		{
			if (buffer.Length > Config.nUdpPackageFixedBodySize)
			{
				NetLog.LogError("发送 广播信息流 溢出");
			}

			NetUdpFixedSizePackage mPackage = mNetPackagePool.Pop();
			mPackage.nPackageId = id;
			Array.Copy(buffer, 0, mPackage.buffer, Config.nUdpPackageFixedHeadSize, buffer.Length);
			mPackage.Length = buffer.Length + Config.nUdpPackageFixedHeadSize;
			NetPackageEncryption.Encryption(mPackage);
			SendNetStream(mPackage.buffer, 0, mPackage.Length);

			mNetPackagePool.recycle(mPackage);
		}
	}
}
