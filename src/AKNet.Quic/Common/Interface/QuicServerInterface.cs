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
using System;
namespace AKNet.Common
{
    public interface QuicServerInterface : IDisposable
    {
        void InitNet();
        void InitNet(int nPort);
        void InitNet(string Ip, int nPort);
        int GetPort();
        SOCKET_SERVER_STATE GetServerState();
        void Update(double elapsed);
        void Update();
        
        void addNetListenFunc(ushort id, Action<QuicClientPeerBase, QuicNetPackage> mFunc);
        void removeNetListenFunc(ushort id, Action<QuicClientPeerBase, QuicNetPackage> mFunc);
        void addNetListenFunc(Action<QuicClientPeerBase, QuicNetPackage> mFunc);
        void removeNetListenFunc(Action<QuicClientPeerBase, QuicNetPackage> mFunc);

        void addListenClientPeerStateFunc(Action<QuicClientPeerBase, SOCKET_PEER_STATE> mFunc);
        void removeListenClientPeerStateFunc(Action<QuicClientPeerBase, SOCKET_PEER_STATE> mFunc);
        void addListenClientPeerStateFunc(Action<QuicClientPeerBase> mFunc);
        void removeListenClientPeerStateFunc(Action<QuicClientPeerBase> mFunc);
    }
}
