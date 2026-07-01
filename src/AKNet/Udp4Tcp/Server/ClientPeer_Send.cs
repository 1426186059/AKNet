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
using System;

namespace AKNet.Udp4Tcp.Server
{
    internal partial class ClientPeer
    {
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
                ReadOnlySpan<byte> mData = mServerMgr.GetCryptoMgr().Encode(id, ReadOnlySpan<byte>.Empty);
                SendNetStream(mData);
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
                SendNetStream(mData);
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