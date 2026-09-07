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
using System.Net;
using System.Runtime.CompilerServices;

namespace KNet.Common
{
    public class JSNetClientMainBase : ClientPeerBase, NetClientInterface
    {
        protected NetClientInterface mInterface = null;
        public NetClientInterface GetInstance()
        {
            return mInterface;
        }

        public void SetInstance(NetClientInterface mInterface)
        {
            this.mInterface = mInterface;
        }

        public void addListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> mFunc) { mInterface.addListenClientPeerStateFunc(mFunc); }
        public void addListenClientPeerStateFunc(Action<ClientPeerBase> mFunc) { mInterface.addListenClientPeerStateFunc(mFunc); }
        public void addNetListenFunc(ushort nPackageId, Action<ClientPeerBase, NetPackage> mFunc) { mInterface.addNetListenFunc(nPackageId, mFunc); }
        public void addNetListenFunc(Action<ClientPeerBase, NetPackage> mFunc) { mInterface.addNetListenFunc(mFunc); }
        public void ConnectServer(string Ip, int nPort) { mInterface.ConnectServer(Ip, nPort); }
        public bool DisConnectServer() { return mInterface.DisConnectServer(); }
        public IPEndPoint GetIPEndPoint() { return mInterface.GetIPEndPoint(); }
        public SOCKET_PEER_STATE GetSocketState() { return mInterface.GetSocketState(); }
        public void ReConnectServer() { mInterface.ReConnectServer(); }
        public void Dispose() { mInterface.Dispose(); }
        public void removeListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> mFunc) { mInterface.removeListenClientPeerStateFunc(mFunc); }
        public void removeListenClientPeerStateFunc(Action<ClientPeerBase> mFunc) { mInterface.removeListenClientPeerStateFunc(mFunc); }
        public void removeNetListenFunc(ushort nPackageId, Action<ClientPeerBase, NetPackage> mFunc) { mInterface.removeNetListenFunc(nPackageId, mFunc); }
        public void removeNetListenFunc(Action<ClientPeerBase, NetPackage> mFunc) { mInterface.removeNetListenFunc(mFunc); }

        public void SendNetData(ushort nPackageId) { mInterface.SendNetData(nPackageId); }
        public void SendNetData(ushort nPackageId, byte[] data) { mInterface.SendNetData(nPackageId, data); }
        public void SendNetData(byte[] data) { mInterface.SendNetData(0, data); }
        public void SendNetData(ArraySegment<byte> data) { mInterface.SendNetData(0, data); }
        public void SendNetData(ushort nPackageId, ArraySegment<byte> data) { mInterface.SendNetData(nPackageId, data); }

        public void Update(double elapsed) { mInterface.Update(elapsed); }
        public void Update() { mInterface.Update(); }

        public void SetName(string name) { mInterface.SetName(name); }
        public string GetName() { return mInterface.GetName(); }
        public void SetID(uint id) { mInterface.SetID(id); }
        public uint GetID() { return mInterface.GetID(); }
        public void SetOwner(object owner) { mInterface.SetOwner(owner); }
        public object GetOwner() { return mInterface.GetOwner(); }
    }
}
