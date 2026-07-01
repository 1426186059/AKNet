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
namespace AKNet.Quic.Client
{
    internal partial class ClientPeer
    {
        public void SendNetData(byte nStreamIndex, ushort nPackageId)
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
            {
                var mStreamObj = GetOrCreateSendStreamHandle(nStreamIndex);
                mStreamObj.SendNetData(nPackageId);
            }
            else
            {
                NetLog.LogError("SendNetData Failed: " + GetSocketState());
            }
        }

        public void SendNetData(byte nStreamIndex, ushort nPackageId, byte[] data)
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
            {
                var mStreamObj = GetOrCreateSendStreamHandle(nStreamIndex);
                mStreamObj.SendNetData(nPackageId, data);
            }
            else
            {
                NetLog.LogError("SendNetData Failed: " + GetSocketState());
            }
        }

        public void SendNetData(byte nStreamIndex, NetPackage mNetPackage)
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
            {
                var mStreamObj = GetOrCreateSendStreamHandle(nStreamIndex);
                mStreamObj.SendNetData(mNetPackage);
            }
            else
            {
                NetLog.LogError("SendNetData Failed: " + GetSocketState());
            }
        }

        public void SendNetData(byte nStreamIndex, ushort nPackageId, ReadOnlySpan<byte> buffer)
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
            {
                var mStreamObj = GetOrCreateSendStreamHandle(nStreamIndex);
                mStreamObj.SendNetData(nPackageId, buffer);
            }
            else
            {
                NetLog.LogError("SendNetData Failed: " + GetSocketState());
            }
        }

        public void SendNetData(byte nStreamIndex, byte[] data) { SendNetData(nStreamIndex, 0, data); }
        public void SendNetData(byte nStreamIndex, ReadOnlySpan<byte> data) { SendNetData(nStreamIndex, 0, data); }
    }
}
