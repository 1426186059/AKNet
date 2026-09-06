// 按 KNet 服务器范式：NetServerMain : NetServerInterface，配合 ClientPeer : ClientPeerBase 与事件分发。
// 底层用第三方库 WebSocketSharp（自带 HttpServer + 自行实现 RFC6455）。
using KNet.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace KNet.WebSocket.Server
{
    public class NetServerMain : NetServerInterface
    {
        private int mPort;
        private SOCKET_SERVER_STATE mState = SOCKET_SERVER_STATE.NONE;
        private WebSocketServer mServer = null;
        public static NetServerMain Current;

        public int GetPort() => mPort;
        public SOCKET_SERVER_STATE GetServerState() => mState;

        private readonly Dictionary<ushort, Action<ClientPeerBase, NetPackage>> mNetEventDic = new Dictionary<ushort, Action<ClientPeerBase, NetPackage>>();
        private Action<ClientPeerBase, NetPackage> mCommonListenFunc;
        private Action<ClientPeerBase, SOCKET_PEER_STATE> mStateFunc1;
        private Action<ClientPeerBase> mStateFunc2;

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
            Current = this;
            mServer = new WebSocketServer(nPort);
            mServer.AddWebSocketService<EchoBehavior>("/");
            mServer.Start();
            NetLog.Log($"[WebSocketSharp] WebSocket 服务器 初始化成功: 0.0.0.0:{nPort}");
        }

        public void Update(double elapsed) { }
        public void Update() { Update(0.016); }

        public void Dispose()
        {
            mState = SOCKET_SERVER_STATE.NONE;
            try { mServer?.Stop(); } catch { }
            mServer = null;
            Current = null;
        }

        public void OnClientConnected(ClientPeer peer)
        {
            peer.SetSocketState(SOCKET_PEER_STATE.CONNECTED);
            mStateFunc2?.Invoke(peer);
            mStateFunc1?.Invoke(peer, SOCKET_PEER_STATE.CONNECTED);
            NetLog.Log($"[WebSocketSharp] 客户端连接: {peer.GetIPEndPoint()}");
        }

        public void OnClientDisconnected(ClientPeer peer)
        {
            peer.SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            mStateFunc2?.Invoke(peer);
            mStateFunc1?.Invoke(peer, SOCKET_PEER_STATE.DISCONNECTED);
            NetLog.Log($"[WebSocketSharp] 客户端断开: {peer.GetIPEndPoint()}");
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

        public class ClientPeer : ClientPeerBase
        {
            private readonly WebSocketBehavior mSocket;
            private SOCKET_PEER_STATE mSocketPeerState = SOCKET_PEER_STATE.DISCONNECTED;
            private IPEndPoint mIPEndPoint = null;
            private string mName = string.Empty;
            private uint mID;
            private object mOwner = null;

            public ClientPeer(WebSocketBehavior socket) { mSocket = socket; }

            public void SetSocketState(SOCKET_PEER_STATE state) { mSocketPeerState = state; }
            public SOCKET_PEER_STATE GetSocketState() => mSocketPeerState;
            public IPEndPoint GetIPEndPoint() => mIPEndPoint;
            internal void SetEndPoint(IPEndPoint ep) { mIPEndPoint = ep; }

            public void SendNetData(ushort nPackageId) { }
            public void SendNetData(ushort nPackageId, byte[] data) { SendBinary(data); }
            public void SendNetData(ushort nPackageId, ReadOnlySpan<byte> buffer) { SendBinary(buffer.ToArray()); }
            public void SendNetData(NetPackage mNetPackage) { SendBinary(mNetPackage.GetData().ToArray()); }
            public void SendNetData(byte[] data) { SendBinary(data); }
            public void SendNetData(ReadOnlySpan<byte> data) { SendBinary(data.ToArray()); }

            private void SendBinary(byte[] data)
            {
                if (mSocketPeerState != SOCKET_PEER_STATE.CONNECTED) return;
                try { mSocket.Context.WebSocket.Send(data); } catch { }
            }

            public void SetName(string name) { mName = name; }
            public string GetName() => mName;
            public void SetID(uint id) { mID = id; }
            public uint GetID() => mID;
            public void SetOwner(object owner) { mOwner = owner; }
            public object GetOwner() => mOwner;

            public void Dispose() { try { mSocket.Context.WebSocket.CloseAsync(); } catch { } }
        }

        internal class NetPackageImpl : NetPackage
        {
            private readonly ushort mId;
            private readonly byte[] mData;
            public NetPackageImpl(ushort id, byte[] data) { mId = id; mData = data; }
            public ushort GetPackageId() => mId;
            public ReadOnlySpan<byte> GetData() => mData;
        }

        // WebSocketSharp 要求无参构造，连接状态通过静态 Current 回传给 NetServerMain
        public class EchoBehavior : WebSocketBehavior
        {
            private ClientPeer mPeer = null;
            protected override void OnOpen()
            {
                var ep = Context.UserEndPoint;
                mPeer = new ClientPeer(this);
                if (ep != null) mPeer.SetEndPoint(new IPEndPoint(ep.Address, ep.Port));
                Current.OnClientConnected(mPeer);
            }
            protected override void OnMessage(MessageEventArgs e)
            {
                if (mPeer != null) Current.Dispatch(mPeer, e.RawData);
            }
            protected override void OnClose(CloseEventArgs e)
            {
                if (mPeer != null) Current.OnClientDisconnected(mPeer);
            }
            protected override void OnError(WebSocketSharp.ErrorEventArgs e) { /* 连接生命周期由 OnClose 处理 */ }
        }
    }
}
