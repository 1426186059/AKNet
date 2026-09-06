// 客户端连接封装（分部类之一：基础状态、属性、生命周期）。
using KNet.Common;
using System;
using System.Net;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace KNet.WebSocket.Server
{
    public partial class ClientPeer : ClientPeerBase
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

        public void SetName(string name) { mName = name; }
        public string GetName() => mName;
        public void SetID(uint id) { mID = id; }
        public uint GetID() => mID;
        public void SetOwner(object owner) { mOwner = owner; }
        public object GetOwner() => mOwner;

        public void Dispose() { try { mSocket.Context.WebSocket.CloseAsync(); } catch { } }
    }
}
