/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库
*        Author:AKNet
*        StartTime:2024/11/01 00:00:00
*        ModifyTime:2026/2/1 20:26:47
*        Copyright:MIT软件许可证
************************************Copyright*****************************************/
using AKNet.Common;
using System;
using System.Net;

namespace AKNet.MSQuic.Server
{
    internal class ClientPeerWrap : QuicClientPeerBase
    {
        internal ClientPeer mInstance = null;
        private ServerMgr mServerMgr;
        public ClientPeerWrap(ServerMgr mNetServer)
        {
            this.mServerMgr = mNetServer;
            this.mInstance = mNetServer.mClientPeerPool.Pop();
        }

        public void Reset()
        {
            mServerMgr.mClientPeerPool.recycle(mInstance);
            mServerMgr = null;
            mInstance = null;
        }

        public SOCKET_PEER_STATE GetSocketState()
        {
            if (mInstance != null) return mInstance.GetSocketState();
            return SOCKET_PEER_STATE.DISCONNECTED;
        }

        public void Update(double elapsed) { if (mInstance != null) mInstance.Update(elapsed); }

        public void SendNetData(byte nStreamIndex, ushort nPackageId) { if (mInstance != null) mInstance.SendNetData(nStreamIndex, nPackageId); }
        public void SendNetData(byte nStreamIndex, ushort nPackageId, byte[] data) { if (mInstance != null) mInstance.SendNetData(nStreamIndex, nPackageId, data); }
        public void SendNetData(byte nStreamIndex, NetPackage mNetPackage) { if (mInstance != null) mInstance.SendNetData(nStreamIndex, mNetPackage); }
        public void SendNetData(byte nStreamIndex, ushort nPackageId, ReadOnlySpan<byte> buffer) { if (mInstance != null) mInstance.SendNetData(nStreamIndex, nPackageId, buffer); }
        public void SendNetData(byte nStreamIndex, byte[] data) { if (mInstance != null) mInstance.SendNetData(nStreamIndex, data); }
        public void SendNetData(byte nStreamIndex, ReadOnlySpan<byte> data) { if (mInstance != null) mInstance.SendNetData(nStreamIndex, data); }

        public IPEndPoint GetIPEndPoint()
        {
            if (mInstance != null) return mInstance.GetIPEndPoint();
            return null;
        }

        public void SetName(string name) { if (mInstance != null) mInstance.SetName(name); }
        public string GetName() { return mInstance != null ? mInstance.GetName() : string.Empty; }
        public void SetID(uint id) { if (mInstance != null) mInstance.SetID(id); }
        public uint GetID() { return mInstance != null ? mInstance.GetID() : 0; }
        public void SetOwner(object owner) { if (mInstance != null) mInstance.SetOwner(owner); }
        public object GetOwner() { return mInstance != null ? mInstance.GetOwner() : null; }
    }
}
