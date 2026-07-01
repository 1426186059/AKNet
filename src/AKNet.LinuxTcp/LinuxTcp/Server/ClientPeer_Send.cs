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
using AKNet.LinuxTcp.Common;
using System;

namespace AKNet.LinuxTcp.Server
{
    internal partial class ClientPeer
    {
        public void SendInnerNetData(byte nInnerCommandId)
        {
            mUdpCheckPool.SendInnerNetData(nInnerCommandId);
        }

        public void SendNetData(ushort nPackageId)
        {
            SendNetData(nPackageId, ReadOnlySpan<byte>.Empty);
        }

        public void SendNetData(ushort nPackageId, byte[] data)
        {
            SendNetData(nPackageId, data.AsSpan());
        }

        public void SendNetData(NetPackage mNetPackage)
        {
            SendNetData(mNetPackage.GetPackageId(), mNetPackage.GetData());
        }

        public void SendNetData(ushort nPackageId, ReadOnlySpan<byte> data)
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
            {
                ReadOnlySpan<byte> mData = mNetServer.GetCryptoMgr().Encode(nPackageId, data);
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
