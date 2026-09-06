/************************************Copyright*****************************************
 *  Project    : KNet
 *  Web        : https://github.com/1426186059/KNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/09/06 00:00:00
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using KNet.Common;

namespace KNet.WebSocket.Server
{
    // 传输层无关的服务器主类（与 KNet.Tcp4 / KNet.WebSocket.Server.TcpListener 同一套书写规则）。
    // 真正完成 HTTP 升级握手的是本文件的分部 NetServerMain_Socket.cs（基于系统 HttpListener + AcceptWebSocketAsync）。
    public partial class NetServerMain : NetServerInterface
    {
        // 复用 KNet.Common 的通用管理器（与客户端/包分发一致）
        internal readonly ListenClientPeerStateMgr mListenClientPeerStateMgr = new ListenClientPeerStateMgr();
        internal readonly ListenNetPackageMgr mPackageManager = new ListenNetPackageMgr();
        internal readonly NetStreamReceivePackage mNetPackage = new NetStreamReceivePackage();
        internal readonly CryptoMgr mCryptoMgr = new CryptoMgr();

        internal readonly ClientPeerPool mClientPeerPool = null;
        private readonly List<ClientPeerWrap> mClientList = new List<ClientPeerWrap>(0);
        private readonly Queue<FakeSocket> mConnectSocketQueue = new Queue<FakeSocket>();

        private int nPort;
        private SOCKET_SERVER_STATE mState = SOCKET_SERVER_STATE.NONE;
        private CancellationTokenSource mCancellationTokenSource = new CancellationTokenSource();
        private System.Net.HttpListener mListener = null;
        private readonly ConfigInstance mConfigInstance;
        private string mBindIp = null;

        public NetServerMain(ConfigInstance mConfig = null)
        {
            this.mConfigInstance = mConfig ?? new ConfigInstance();
            this.mClientPeerPool = new ClientPeerPool(this, 0, this.mConfigInstance.MaxPlayerCount);
        }

        public void OnSocketStateChanged(ClientPeerBase mClientPeer)
        {
            mListenClientPeerStateMgr.OnSocketStateChanged(mClientPeer);
        }

        public void addListenClientPeerStateFunc(Action<ClientPeerBase> mFunc)
        {
            mListenClientPeerStateMgr.addListenClientPeerStateFunc(mFunc);
        }

        public void removeListenClientPeerStateFunc(Action<ClientPeerBase> mFunc)
        {
            mListenClientPeerStateMgr.removeListenClientPeerStateFunc(mFunc);
        }

        public void addListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> mFunc)
        {
            mListenClientPeerStateMgr.addListenClientPeerStateFunc(mFunc);
        }

        public void removeListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> mFunc)
        {
            mListenClientPeerStateMgr.removeListenClientPeerStateFunc(mFunc);
        }

        public void addNetListenFunc(ushort id, Action<ClientPeerBase, NetPackage> func)
        {
            mPackageManager.addNetListenFunc(id, func);
        }

        public void removeNetListenFunc(ushort id, Action<ClientPeerBase, NetPackage> func)
        {
            mPackageManager.removeNetListenFunc(id, func);
        }

        public void addNetListenFunc(Action<ClientPeerBase, NetPackage> func)
        {
            mPackageManager.addNetListenFunc(func);
        }

        public void removeNetListenFunc(Action<ClientPeerBase, NetPackage> func)
        {
            mPackageManager.removeNetListenFunc(func);
        }

        FrameUpdateFunc mFrameUpdateFunc = null;
        public void Update()
        {
            if (mFrameUpdateFunc == null)
            {
                mFrameUpdateFunc = new FrameUpdateFunc();
            }
            mFrameUpdateFunc.Update(Update);
        }

        public int GetPort() => nPort;
        public SOCKET_SERVER_STATE GetServerState() => mState;

        public void Dispose()
        {
            CloseNet();
        }

        public void CloseNet()
        {
            MainThreadCheck.Check();
            mCancellationTokenSource?.Cancel();

            if (mListener != null)
            {
                try { mListener.Stop(); } catch { }
                mListener = null;
            }

            lock (mConnectSocketQueue)
            {
                while (mConnectSocketQueue.Count > 0)
                {
                    mConnectSocketQueue.Dequeue().mWebSocket.Dispose();
                }
            }

            for (int i = mClientList.Count - 1; i >= 0; i--)
            {
                mClientList[i].Dispose();
            }
            mClientList.Clear();

            mState = SOCKET_SERVER_STATE.NONE;
        }
    }
}
