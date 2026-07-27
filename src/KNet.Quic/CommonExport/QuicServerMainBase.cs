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
namespace KNet.Common
{
    public class QuicServerMainBase : QuicServerInterface
    {
        protected QuicServerInterface mInterface = null;
        public QuicServerInterface GetInstance()
        {
            return mInterface;
        }

        public void SetInstance(QuicServerInterface mInterface)
        {
            this.mInterface = mInterface;
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

        public void addListenClientPeerStateFunc(Action<QuicClientPeerBase, SOCKET_PEER_STATE> mFunc)
        {
            mInterface.addListenClientPeerStateFunc(mFunc);
        }

        public void addListenClientPeerStateFunc(Action<QuicClientPeerBase> mFunc)
        {
            mInterface.addListenClientPeerStateFunc(mFunc);
        }

        public void removeListenClientPeerStateFunc(Action<QuicClientPeerBase, SOCKET_PEER_STATE> mFunc)
        {
            mInterface.removeListenClientPeerStateFunc(mFunc);
        }

        public void removeListenClientPeerStateFunc(Action<QuicClientPeerBase> mFunc)
        {
            mInterface.removeListenClientPeerStateFunc(mFunc);
        }

   

        public void Update(double elapsed)
        {
            mInterface.Update(elapsed);
        }

        public void Update() { mInterface.Update(); }

        public void addNetListenFunc(ushort id, Action<QuicClientPeerBase, QuicNetPackage> mFunc)
        {
            mInterface.addNetListenFunc(id, mFunc);
        }

        public void removeNetListenFunc(ushort id, Action<QuicClientPeerBase, QuicNetPackage> mFunc)
        {
            mInterface.removeNetListenFunc(id, mFunc);
        }

        public void addNetListenFunc(Action<QuicClientPeerBase, QuicNetPackage> mFunc)
        {
            mInterface.addNetListenFunc(mFunc);
        }

        public void removeNetListenFunc(Action<QuicClientPeerBase, QuicNetPackage> mFunc)
        {
            mInterface.removeNetListenFunc(mFunc);
        }
    }
}
