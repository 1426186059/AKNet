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
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("KNet")]
[assembly: InternalsVisibleTo("KNet.MSQuic")]
[assembly: InternalsVisibleTo("KNet.LinuxTcp")]
[assembly: InternalsVisibleTo("KNet.WebSocket")]
namespace KNet.Common
{
    public class NetServerMainBase : NetServerInterface
    {
        protected NetServerInterface mInterface = null;
        public NetServerInterface GetInstance()
        {
            return mInterface;
        }

        public void SetInstance(NetServerInterface mInterface)
        {
            this.mInterface = mInterface;
        }

        public void addListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> mFunc)
        {
            mInterface.addListenClientPeerStateFunc(mFunc);
        }

        public void addListenClientPeerStateFunc(Action<ClientPeerBase> mFunc)
        {
            mInterface.addListenClientPeerStateFunc(mFunc);
        }

        public void addNetListenFunc(ushort nPackageId, Action<ClientPeerBase, NetPackage> mFunc)
        {
            mInterface.addNetListenFunc(nPackageId, mFunc);
        }

        public void addNetListenFunc(Action<ClientPeerBase, NetPackage> mFunc)
        {
            mInterface.addNetListenFunc(mFunc);
        }

        public int GetPort()
        {
            return mInterface.GetPort();
        }

        public SOCKET_SERVER_STATE GetServerState()
        {
            return mInterface.GetServerState();
        }

        public void InitNet()
        {
            mInterface.InitNet();
        }

        public void InitNet(int nPort)
        {
            mInterface.InitNet(nPort);
        }

        public void InitNet(string Ip, int nPort)
        {
            mInterface.InitNet(Ip, nPort);
        }

        public void Dispose()
        {
            mInterface.Dispose();
        }

        public void removeListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> mFunc)
        {
            mInterface.removeListenClientPeerStateFunc(mFunc);
        }

        public void removeListenClientPeerStateFunc(Action<ClientPeerBase> mFunc)
        {
            mInterface.removeListenClientPeerStateFunc(mFunc);
        }

        public void removeNetListenFunc(ushort nPackageId, Action<ClientPeerBase, NetPackage> mFunc)
        {
            mInterface.removeNetListenFunc(nPackageId, mFunc);
        }

        public void removeNetListenFunc(Action<ClientPeerBase, NetPackage> mFunc)
        {
            mInterface.removeNetListenFunc(mFunc);
        }

        public void Update(double elapsed)
        {
            mInterface.Update(elapsed);
        }

        public void Update() { mInterface.Update(); }
    }
}
