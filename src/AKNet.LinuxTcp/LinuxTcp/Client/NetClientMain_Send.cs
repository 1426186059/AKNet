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

namespace AKNet.LinuxTcp.Client
{
    internal partial class NetClientMain
    {
        public void SendInnerNetData(byte nInnerCommandId)
        {
            mUdpCheckPool.SendInnerNetData(nInnerCommandId);
        }

        public void SendNetData(NetPackage mNetPackage)
        {
            SendNetData(mNetPackage.GetPackageId(), mNetPackage.GetData());
        }

        public void SendNetData(UInt16 nLogicPackageId)
        {
            SendNetData(nLogicPackageId, ReadOnlySpan<byte>.Empty);
        }

        public void SendNetData(UInt16 nLogicPackageId, byte[] data)
        {
            SendNetData(nLogicPackageId, data.AsSpan());
        }

        public void SendNetData(UInt16 nLogicPackageId, ReadOnlySpan<byte> data)
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
            {
                ReadOnlySpan<byte> mData = mCryptoMgr.Encode(nLogicPackageId, data);
                mUdpCheckPool.SendTcpStream(mData);
            }
        }
    }
}
