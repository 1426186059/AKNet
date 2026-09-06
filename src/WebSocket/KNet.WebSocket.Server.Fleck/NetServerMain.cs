// 按 KNet 服务器范式：NetServerMain : NetServerInterface，配合 ClientPeer : ClientPeerBase 与事件分发。
// 底层用第三方库 Fleck（在 TCP 上自行实现 RFC6455 握手与帧）。
using KNet.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Fleck;

namespace KNet.WebSocket.Server
{
    public class NetServerMain : NetServerInterface
    {
        private int mPort;
        private SOCKET_SERVER_STATE mState = SOCKET_SERVER_STATE.NONE;
        private WebSocketServer mServer = null;

        public int GetPort() => mPort;
        public SOCKET_SERVER_STATE GetServerState() => mState;

        // KNet 内部用 ListenNetPackageMgr / ListenClientPeerStateMgr（KNet.Common 友元），这里独立工程自实现等价分发
        private readonly Dictionary<ushort, Action<ClientPeerBase, NetPackage>> mNetEventDic = new Dictionary<ushort, Action<ClientPeerBase, NetPackage>>();
        private Action<ClientPeerBase, NetPackage> mCommonListenFunc = null;
        private Action<ClientPeerBase, SOCKET_PEER_STATE> mStateFunc1 = null;
        private Action<ClientPeerBase> mStateFunc2 = null;

        public void addNetListenFunc(Action<ClientPeerBase, NetPackage> func) { mCommonListenFunc += func; }
        public void removeNetListenFunc(Action<ClientPeerBase, NetPackage> func) { mCommonListenFunc -= func; }
        public void addNetListenFunc(ushort id, Action<ClientPeerBase, NetPackage> func)
        {
            if (!mNetEventDic.ContainsKey(id)) mNetEventDic[id] = func; else mNetEventDic[id] += func;
        }
        public void removeNetListenFunc(ushort id, Action<ClientPeerBase, NetPackage> func) { if (mNetEventDic.ContainsKey(id)) mNetEventDic[id] -= func; }
        public void addListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> func) { mStateFunc1 += func; }
        public void removeListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> func) { mStateFunc1 -= func; }
        public void addListenClientPeerStateFunc(Action<ClientPeerBase> func) { mStateFunc2 += func; }
        public void removeListenClientPeerStateFunc(Action<ClientPeerBase> func) { mStateFunc2 -= func; }

        public void InitNet() { InitNet(GetFreePort()); }
        public void InitNet(int nPort) { InitNet(IPAddress.Any.ToString(), nPort); }
        public void InitNet(string Ip, int nPort)
        {
            mPort = nPort;
            mState = SOCKET_SERVER_STATE.NORMAL;
            mServer = new WebSocketServer($"ws://0.0.0.0:{nPort}");
            mServer.Start(socket =>
            {
                var peer = new ClientPeer(socket);
                socket.OnOpen = () =>
                {
                    peer.SetEndPoint(new IPEndPoint(IPAddress.Parse(socket.ConnectionInfo.ClientIpAddress), socket.ConnectionInfo.ClientPort));
                    OnClientConnected(peer);
                };
                socket.OnClose = () => OnClientDisconnected(peer);
                socket.OnBinary = bytes => Dispatch(peer, bytes);
                socket.OnMessage = text => Dispatch(peer, Encoding.UTF8.GetBytes(text));
            });
            NetLog.Log($"[Fleck] WebSocket 服务器 初始化成功: 0.0.0.0:{nPort}");
        }

        public void Update(double elapsed) { }
        public void Update() { Update(0.016); }

        public void Dispose()
        {
            mState = SOCKET_SERVER_STATE.NONE;
            try { mServer?.Dispose(); } catch { }
            mServer = null;
        }

        public void OnClientConnected(ClientPeer peer)
        {
            peer.SetSocketState(SOCKET_PEER_STATE.CONNECTED);
            mStateFunc2?.Invoke(peer);
            mStateFunc1?.Invoke(peer, SOCKET_PEER_STATE.CONNECTED);
            NetLog.Log($"[Fleck] 客户端连接: {peer.GetIPEndPoint()}");
        }

        public void OnClientDisconnected(ClientPeer peer)
        {
            peer.SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            mStateFunc2?.Invoke(peer);
            mStateFunc1?.Invoke(peer, SOCKET_PEER_STATE.DISCONNECTED);
            NetLog.Log($"[Fleck] 客户端断开: {peer.GetIPEndPoint()}");
        }

        public void Dispatch(ClientPeer peer, byte[] data)
        {
            var pkg = new NetPackageImpl(0, data);
            if (mCommonListenFunc != null) mCommonListenFunc(peer, pkg);
            else if (mNetEventDic.TryGetValue(pkg.GetPackageId(), out var func) && func != null) func(peer, pkg);
        }

        private static int GetFreePort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int p = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return p;
        }
    }
}
