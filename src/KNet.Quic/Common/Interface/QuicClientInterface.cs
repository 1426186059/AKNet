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
    public interface QuicClientInterface : IDisposable
    {
        void ConnectServer(string Ip, int nPort);
        bool DisConnectServer();
        void ReConnectServer();
        void Update(double elapsed);
        void Update();
        
        void addNetListenFunc(ushort nPackageId, Action<QuicClientPeerBase, QuicNetPackage> mFunc);
        void removeNetListenFunc(ushort nPackageId, Action<QuicClientPeerBase, QuicNetPackage> mFunc);
        void addNetListenFunc(Action<QuicClientPeerBase, QuicNetPackage> mFunc);
        void removeNetListenFunc(Action<QuicClientPeerBase, QuicNetPackage> mFunc);

        void addListenClientPeerStateFunc(Action<QuicClientPeerBase, SOCKET_PEER_STATE> mFunc);
        void removeListenClientPeerStateFunc(Action<QuicClientPeerBase, SOCKET_PEER_STATE> mFunc);
        void addListenClientPeerStateFunc(Action<QuicClientPeerBase> mFunc);
        void removeListenClientPeerStateFunc(Action<QuicClientPeerBase> mFunc);
        IPEndPoint GetIPEndPoint();
        SOCKET_PEER_STATE GetSocketState();
        
        void SendNetData(byte nStreamIndex, ushort nPackageId);
        void SendNetData(byte nStreamIndex, ushort nPackageId, byte[] data);
        void SendNetData(byte nStreamIndex, ushort nPackageId, ReadOnlySpan<byte> buffer);
        void SendNetData(byte nStreamIndex, NetPackage mNetPackage);

        void SetName(string name);
        string GetName();
        void SetID(uint id);
        uint GetID();
        void SetOwner(object owner);
        object GetOwner();
    }
}
