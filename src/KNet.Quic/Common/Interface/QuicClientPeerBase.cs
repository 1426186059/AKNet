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
using System;
using System.Net;

namespace KNet.Common
{
    public interface QuicClientPeerBase: IDisposable
    {
        IPEndPoint GetIPEndPoint();
        SOCKET_PEER_STATE GetSocketState();
        void SendNetData(byte nStreamIndex, ushort nPackageId);
        void SendNetData(byte nStreamIndex, ushort nPackageId, byte[] data);
        void SendNetData(byte nStreamIndex, ushort nPackageId, ReadOnlySpan<byte> buffer);
        void SendNetData(byte nStreamIndex, NetPackage mNetPackage);
        void SendNetData(byte nStreamIndex, byte[] data);
        void SendNetData(byte nStreamIndex, ReadOnlySpan<byte> data);

        void SetName(string name);
        string GetName();
        void SetID(uint id);
        uint GetID();
        void SetOwner(object owner);
        object GetOwner();
    }
}
