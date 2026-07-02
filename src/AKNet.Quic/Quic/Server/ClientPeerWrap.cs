/************************************Copyright*****************************************
 *  Project    : AKNet
 *  Web        : https://github.com/1426186059/AKNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/07/02 18:05:45
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using AKNet.Common;
using System;
using System.Net;

namespace AKNet.Quic.Server
{
    internal class ClientPeerWrap : QuicClientPeerBase
    {
        internal ClientPeer mInstance = null;
        private ServerMgr mServerMgr;
        public ClientPeerWrap(ServerMgr mNetServer)
        {
            this.mServerMgr = mNetServer;
            this.mInstance = mNetServer.mClientPeerPool.Pop();
            this.mInstance.SetWrap(this);
        }

        public void Reset()
        {
            if (mInstance != null)
            {
                mServerMgr.mClientPeerPool.recycle(mInstance);
                mServerMgr = null;
                mInstance = null;
            }
        }

        public void Dispose() 
        { 
            if(mInstance != null)
            {
                mInstance.SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
                Reset();
            }
        }

        public SOCKET_PEER_STATE GetSocketState()
        {
            if (mInstance != null) return mInstance.GetSocketState();
            return SOCKET_PEER_STATE.DISCONNECTED;
        }

        public void Update(double elapsed)
        {
            if (mInstance != null) mInstance.Update(elapsed);
        }

        public void SendNetData(byte nStreamIndex, ushort nPackageId)
        {
            if (mInstance != null) mInstance.SendNetData(nStreamIndex, nPackageId);
        }

        public void SendNetData(byte nStreamIndex, ushort nPackageId, byte[] data)
        {
            if (mInstance != null) mInstance.SendNetData(nStreamIndex, nPackageId, data);
        }

        public void SendNetData(byte nStreamIndex, NetPackage mNetPackage)
        {
            if (mInstance != null) mInstance.SendNetData(nStreamIndex, mNetPackage);
        }

        public void SendNetData(byte nStreamIndex, ushort nPackageId, ReadOnlySpan<byte> buffer)
        {
            if (mInstance != null) mInstance.SendNetData(nStreamIndex, nPackageId, buffer);
        }

        public void SendNetData(byte nStreamIndex, byte[] data)
        {
            if (mInstance != null) mInstance.SendNetData(nStreamIndex, data);
        }

        public void SendNetData(byte nStreamIndex, ReadOnlySpan<byte> data)
        {
            if (mInstance != null) mInstance.SendNetData(nStreamIndex, data);
        }

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
