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
using System;

namespace KNet.WebSocket.Client
{
    public partial class NetClientMain
    {
        public void SendNetData(ushort nPackageId)
        {
            if (this.mSocketPeerState == SOCKET_PEER_STATE.CONNECTED)
            {
                ReadOnlySpan<byte> mBufferSegment = mCryptoMgr.Encode(nPackageId, ReadOnlySpan<byte>.Empty);
                SendNetStream(mBufferSegment);
            }
            else { NetLog.LogError("SendNetData Failed: " + GetSocketState()); }
        }

        public void SendNetData(ushort nPackageId, byte[] data)
        {
            if (this.mSocketPeerState == SOCKET_PEER_STATE.CONNECTED)
            {
                ReadOnlySpan<byte> mBufferSegment = mCryptoMgr.Encode(nPackageId, data);
                SendNetStream(mBufferSegment);
            }
            else { NetLog.LogError("SendNetData Failed: " + GetSocketState()); }
        }

        public void SendNetData(NetPackage mNetPackage)
        {
            if (this.mSocketPeerState == SOCKET_PEER_STATE.CONNECTED)
            {
                ReadOnlySpan<byte> mBufferSegment = mCryptoMgr.Encode(mNetPackage.GetPackageId(), mNetPackage.GetData());
                SendNetStream(mBufferSegment);
            }
            else { NetLog.LogError("SendNetData Failed: " + GetSocketState()); }
        }

        public void SendNetData(ushort nPackageId, ReadOnlySpan<byte> buffer)
        {
            if (this.mSocketPeerState == SOCKET_PEER_STATE.CONNECTED)
            {
                ReadOnlySpan<byte> mBufferSegment = mCryptoMgr.Encode(nPackageId, buffer);
                SendNetStream(mBufferSegment);
            }
            else { NetLog.LogError("SendNetData Failed: " + GetSocketState()); }
        }

        public void SendNetData(byte[] data)
        {
            if (this.mSocketPeerState == SOCKET_PEER_STATE.CONNECTED)
            {
                ReadOnlySpan<byte> mBufferSegment = mCryptoMgr.Encode(0, data);
                SendNetStream(mBufferSegment);
            }
            else { NetLog.LogError("SendNetData Failed: " + GetSocketState()); }
        }

        public void SendNetData(ReadOnlySpan<byte> data)
        {
            if (this.mSocketPeerState == SOCKET_PEER_STATE.CONNECTED)
            {
                ReadOnlySpan<byte> mBufferSegment = mCryptoMgr.Encode(0, data);
                SendNetStream(mBufferSegment);
            }
            else { NetLog.LogError("SendNetData Failed: " + GetSocketState()); }
        }
    }
}
