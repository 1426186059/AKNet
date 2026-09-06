// 客户端连接封装（分部类之一：基础状态、属性、生命周期）。
using KNet.Common;
using System;
using System.Net;
using System.Net.WebSockets;

namespace KNet.WebSocket.Server
{
    public partial class ClientPeer : ClientPeerBase
    {
        private readonly System.Net.WebSockets.WebSocket mWs;
        private readonly NetServerMain mServerMgr;
        private SOCKET_PEER_STATE mSocketPeerState = SOCKET_PEER_STATE.DISCONNECTED;
        private IPEndPoint mIPEndPoint = null;
        private string mName = string.Empty;
        private uint mID;
        private object mOwner = null;

        // 逐连接独立的 KNet 编解码上下文（不跨连接共享，避免 Encode/Decode 内部缓冲被并发改写）
        private readonly CryptoMgr mCryptoMgr = new CryptoMgr();
        private readonly NetStreamCircularBuffer mReceiveStreamList = new NetStreamCircularBuffer();
        private readonly NetStreamReceivePackage mNetPackage = new NetStreamReceivePackage();

        public ClientPeer(System.Net.WebSockets.WebSocket ws, IPEndPoint ep, NetServerMain serverMgr)
        {
            mWs = ws;
            mIPEndPoint = ep;
            mServerMgr = serverMgr;
        }

        public void SetSocketState(SOCKET_PEER_STATE state) { mSocketPeerState = state; }
        public SOCKET_PEER_STATE GetSocketState() => mSocketPeerState;
        public IPEndPoint GetIPEndPoint() => mIPEndPoint;

        public void SetName(string name) { mName = name; }
        public string GetName() => mName;
        public void SetID(uint id) { mID = id; }
        public uint GetID() => mID;
        public void SetOwner(object owner) { mOwner = owner; }
        public object GetOwner() => mOwner;

        public void Dispose() { try { mWs.Dispose(); } catch { } }
    }
}
