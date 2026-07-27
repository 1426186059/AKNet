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
using System;

namespace KNet.Udp1Tcp.Client
{
    internal partial class NetClientMain
    {
		public void SendInnerNetData(UInt16 id)
		{
			NetLog.Assert(UdpNetCommand.orInnerCommand(id));
			NetUdpFixedSizePackage mPackage = GetObjectPoolManager().NetUdpFixedSizePackage_Pop();
			mPackage.nPackageId = id;
			mPackage.Length = Config.nUdpPackageFixedHeadSize;
			SendNetPackage(mPackage);
		}

		public void SendNetData(NetPackage mNetPackage)
		{
			if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
			{
                SendNetData(mNetPackage.GetPackageId(), mNetPackage.GetData());
            }
			else
			{
				NetLog.LogError("SendNetData Failed: " + GetSocketState());
			}
		}

        public void SendNetData(UInt16 id)
		{
			if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
			{
				NetLog.Assert(UdpNetCommand.orNeedCheck(id));
				mUdpCheckPool.SendLogicPackage(id, ReadOnlySpan<byte>.Empty);
			}
            else
            {
                NetLog.LogError("SendNetData Failed: " + GetSocketState());
            }
        }

		public void SendNetData(UInt16 id, byte[] data)
		{
			SendNetData(id, data.AsSpan());
		}

        public void SendNetData(UInt16 id, ReadOnlySpan<byte> data)
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
            {
                NetLog.Assert(UdpNetCommand.orNeedCheck(id));
                mUdpCheckPool.SendLogicPackage(id, data);
            }
            else
            {
                NetLog.LogError("SendNetData Failed: " + GetSocketState());
            }
        }

        public void SendNetData(byte[] data)
        {
            SendNetData(0, data);
        }

        public void SendNetData(ReadOnlySpan<byte> data)
        {
            SendNetData(0, data);
        }
    }
}