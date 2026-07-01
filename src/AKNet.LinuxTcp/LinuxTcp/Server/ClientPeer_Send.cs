/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库
*        Author:许珂
*        StartTime:2024/11/01 00:00:00
*        ModifyTime:2026/2/1 20:27:10
*        Copyright:MIT软件许可证
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
                ReadOnlySpan<byte> mData = mNetServer.mCryptoMgr.Encode(nPackageId, data);
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
