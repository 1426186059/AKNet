/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库 - TcpListener 实现
*        Author:许珂
*        CreateTime:2026/6/28
*        Copyright:MIT软件许可证
************************************Copyright*****************************************/
using AKNet.Common;
using System;
using System.Net;
using System.Net.Sockets;

namespace AKNet.Tcp2.Server
{
    // 这个类非常有必要，因为 ClientPeer 是复用的，我们得让上层逻辑知道 ClientPeer 是否存活。
    // 加这个简单的空壳类解决了所有问题。
    internal class ClientPeerWrap : ClientPeerBase
    {
        private ClientPeer mInstance = null;
        private NetServerMain mServerMgr;
        public ClientPeerWrap(NetServerMain mNetServer)
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
            if (mInstance != null)
            {
                return mInstance.GetSocketState();
            }
            else
            {
                return SOCKET_PEER_STATE.DISCONNECTED;
            }
        }

        public void Update(double elapsed)
        {
            if (mInstance != null)
            {
                mInstance.Update(elapsed);
            }
        }

        public void SendNetData(ushort nPackageId)
        {
            if (mInstance != null)
            {
                mInstance.SendNetData(nPackageId);
            }
        }

        public void SendNetData(ushort nPackageId, byte[] data)
        {
            if (mInstance != null)
            {
                mInstance.SendNetData(nPackageId, data);
            }
        }

        public void SendNetData(NetPackage data)
        {
            if (mInstance != null)
            {
                mInstance.SendNetData(data);
            }
        }

        public void SendNetData(ushort nPackageId, ReadOnlySpan<byte> data)
        {
            if (mInstance != null)
            {
                mInstance.SendNetData(nPackageId, data);
            }
        }

        public void HandleConnectedTcpClient(TcpClient client)
        {
            if (mInstance != null)
            {
                mInstance.HandleConnectedTcpClient(client);
            }
        }

        public IPEndPoint GetIPEndPoint()
        {
            if (mInstance != null)
            {
                return mInstance.GetIPEndPoint();
            }
            return null;
        }

        public void SetName(string name)
        {
            mInstance.SetName(name);
        }

        public string GetName()
        {
            return mInstance.GetName();
        }

        public void SetID(uint id)
        {
            mInstance.SetID(id);
        }

        public uint GetID()
        {
            return mInstance.GetID();
        }
        public void SetOwner(object owner) { if (mInstance != null) mInstance.SetOwner(owner); }
        public object GetOwner() { return mInstance != null ? mInstance.GetOwner() : null; }
        public void SendNetData(byte[] data) { if (mInstance != null) mInstance.SendNetData(data); }
        public void SendNetData(ReadOnlySpan<byte> data) { if (mInstance != null) mInstance.SendNetData(data); }
    }
}
