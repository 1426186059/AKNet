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
using KNet.Udp3Tcp.Common;
using System;

namespace KNet.Udp3Tcp.Server
{
    internal partial class ClientPeer
    {
        public void SendInnerNetData(byte id)
        {
            NetLog.Assert(UdpNetCommand.orInnerCommand(id));
            NetUdpSendFixedSizePackage mPackage = GetObjectPoolManager().UdpSendPackage_Pop();
            mPackage.SetInnerCommandId(id);
            SendNetPackage(mPackage);
            GetObjectPoolManager().UdpSendPackage_Recycle(mPackage);
        }

        public void SendNetData(NetPackage mNetPackage)
        {
            if (mSocketPeerState == SOCKET_PEER_STATE.CONNECTED)
            {
                SendNetData(mNetPackage.GetPackageId(), mNetPackage.GetData());
            }
        }

        public void SendNetData(UInt16 id)
        {
            if (mSocketPeerState == SOCKET_PEER_STATE.CONNECTED)
            {
                mUdpCheckPool.SendTcpStream(ReadOnlySpan<byte>.Empty);
            }
        }

        public void SendNetData(UInt16 id, byte[] data)
        {
            SendNetData(id, data.AsSpan());
        }

        public void SendNetData(UInt16 id, ReadOnlySpan<byte> data)
        {
            if (mSocketPeerState == SOCKET_PEER_STATE.CONNECTED)
            {
                ReadOnlySpan<byte> mData = mServerMgr.GetCryptoMgr().Encode(id, data);
                mUdpCheckPool.SendTcpStream(mData);
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