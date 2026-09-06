// 按 KNet 服务器范式：NetServerMain : NetServerInterface，配合 ClientPeer : ClientPeerBase 与事件分发。
// 底层用 BCL HttpListener 完成官方 HTTP 升级握手（零第三方依赖，天然支持 wss）。
using KNet.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace KNet.WebSocket.Server
{
    public partial class NetServerMain : NetServerInterface
    {
        // 复用 KNet.Common 的通用管理器（与 TcpListener 一致）
        internal readonly ListenClientPeerStateMgr mListenClientPeerStateMgr = new ListenClientPeerStateMgr();
        internal readonly ListenNetPackageMgr mPackageManager = new ListenNetPackageMgr();

        // ClientPeer 管理器：登记活跃连接，超时/断开时移除
        private readonly List<ClientPeer> mClientList = new List<ClientPeer>();
        private readonly object mClientListLock = new object();
        private Timer mUpdateTimer = null;

        private int mPort;
        private SOCKET_SERVER_STATE mState = SOCKET_SERVER_STATE.NONE;
        private System.Net.HttpListener mListener = null;
        private readonly CancellationTokenSource mCts = new CancellationTokenSource();

        public int GetPort() => mPort;
        public SOCKET_SERVER_STATE GetServerState() => mState;
        public int GetClientCount() { lock (mClientListLock) return mClientList.Count; }

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
            try { mUpdateTimer?.Dispose(); } catch { }
            mUpdateTimer = null;
            try { mCts.Cancel(); } catch { }
            try { mListener?.Stop(); } catch { }
            mListener = null;
        }
    }
}
