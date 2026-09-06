// 按 KNet 服务器范式：NetServerMain : NetServerInterface，配合 ClientPeer : ClientPeerBase 与事件分发。
// 底层用第三方库 Fleck（在 TCP 上自行实现 RFC6455 握手与帧）。
using Fleck;
using KNet.Common;

namespace KNet.WebSocket.Server
{
    public partial class NetServerMain : NetServerInterface
    {
        // 复用 KNet.Common 的通用管理器（与 TcpListener 一致）
        internal readonly ListenClientPeerStateMgr mListenClientPeerStateMgr = new ListenClientPeerStateMgr();
        internal readonly ListenNetPackageMgr mPackageManager = new ListenNetPackageMgr();

        // ClientPeer 管理器：登记活跃连接，超时/断开时移除
        private readonly Queue<ClientPeer> mConnectSocketQueue = new Queue<ClientPeer>();
        private readonly List<ClientPeerWrap> mClientList = new List<ClientPeerWrap>();

        private int mPort;
        private SOCKET_SERVER_STATE mState = SOCKET_SERVER_STATE.NONE;
        private WebSocketServer mServer = null;

        private readonly ConfigInstance mConfigInstance;
        internal readonly ClientPeerPool mClientPeerPool;
        public NetServerMain(ConfigInstance mConfig = null)
        {
            this.mConfigInstance = mConfig ?? new ConfigInstance();
            this.mClientPeerPool = new ClientPeerPool(this, 0, this.mConfigInstance.MaxPlayerCount);
        }


        public int GetPort() => mPort;
        public SOCKET_SERVER_STATE GetServerState() => mState;
        public int GetClientCount() { return mClientList.Count; }

        // 包监听：委托给 mPackageManager
        public void addNetListenFunc(ushort id, Action<ClientPeerBase, NetPackage> func) { mPackageManager.addNetListenFunc(id, func); }
        public void removeNetListenFunc(ushort id, Action<ClientPeerBase, NetPackage> func) { mPackageManager.removeNetListenFunc(id, func); }
        public void addNetListenFunc(Action<ClientPeerBase, NetPackage> func) { mPackageManager.addNetListenFunc(func); }
        public void removeNetListenFunc(Action<ClientPeerBase, NetPackage> func) { mPackageManager.removeNetListenFunc(func); }

        // 状态监听：委托给 mListenClientPeerStateMgr
        public void addListenClientPeerStateFunc(Action<ClientPeerBase> func) { mListenClientPeerStateMgr.addListenClientPeerStateFunc(func); }
        public void removeListenClientPeerStateFunc(Action<ClientPeerBase> func) { mListenClientPeerStateMgr.removeListenClientPeerStateFunc(func); }
        public void addListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> func) { mListenClientPeerStateMgr.addListenClientPeerStateFunc(func); }
        public void removeListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> func) { mListenClientPeerStateMgr.removeListenClientPeerStateFunc(func); }

        public void OnSocketStateChanged(ClientPeerBase peer) { mListenClientPeerStateMgr.OnSocketStateChanged(peer); }

        // 入参已是按 KNet 协议解码后的包
        public void Dispatch(ClientPeerBase peer, NetPackage pkg) { mPackageManager.NetPackageExecute(peer, pkg); }

        public void Update() { Update(0.016); }

        public void Dispose()
        {
            mState = SOCKET_SERVER_STATE.NONE;
            try { mServer?.Dispose(); } catch { }
            mServer = null;
        }
    }
}
