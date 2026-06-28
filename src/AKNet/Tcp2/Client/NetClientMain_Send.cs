/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库 - TcpClient 实现
*        Author:许珂
*        CreateTime:2026/6/28
*        Copyright:MIT软件许可证
************************************Copyright*****************************************/
using AKNet.Common;
using System;

namespace AKNet.Tcp2.Client
{
    internal partial class NetClientMain
    {
        public void SendNetData(ushort nPackageId)
        {
            if (this.mSocketPeerState == SOCKET_PEER_STATE.CONNECTED)
            {
                ReadOnlySpan<byte> mBufferSegment = mCryptoMgr.Encode(nPackageId, ReadOnlySpan<byte>.Empty);
                SendNetStream(mBufferSegment);
            }
            else
            {
                NetLog.LogError("SendNetData Failed: " + GetSocketState());
            }
        }

        public void SendNetData(ushort nPackageId, byte[] data)
        {
            if (this.mSocketPeerState == SOCKET_PEER_STATE.CONNECTED)
            {
                ReadOnlySpan<byte> mBufferSegment = mCryptoMgr.Encode(nPackageId, data);
                SendNetStream(mBufferSegment);
            }
            else
            {
                NetLog.LogError("SendNetData Failed: " + GetSocketState());
            }
        }

        public void SendNetData(NetPackage mNetPackage)
        {
            if (this.mSocketPeerState == SOCKET_PEER_STATE.CONNECTED)
            {
                ReadOnlySpan<byte> mBufferSegment = mCryptoMgr.Encode(mNetPackage.GetPackageId(), mNetPackage.GetData());
                SendNetStream(mBufferSegment);
            }
            else
            {
                NetLog.LogError("SendNetData Failed: " + GetSocketState());
            }
        }

        public void SendNetData(ushort nPackageId, ReadOnlySpan<byte> buffer)
        {
            if (this.mSocketPeerState == SOCKET_PEER_STATE.CONNECTED)
            {
                ReadOnlySpan<byte> mBufferSegment = mCryptoMgr.Encode(nPackageId, buffer);
                SendNetStream(mBufferSegment);
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
