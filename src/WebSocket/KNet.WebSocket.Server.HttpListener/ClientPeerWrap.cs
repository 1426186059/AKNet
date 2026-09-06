/************************************Copyright*****************************************
 *  Project    : KNet
 *  Web        : https://github.com/1426186059/KNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/09/06 00:00:00
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using KNet.Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace KNet.WebSocket.Server
{
    //这个类非常有必要，因为 ClientPeer 是复用的，我们得让 上层逻辑知道 我这个CientPeer 是否 存活。
    //加这个 简单的空壳类 解决了所有问题。
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
            if (mInstance != null)
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

        // 系统 HttpListener 完成 HTTP 升级后，由 NetServerMain 调用，把已建立的 WebSocket 交给底层 ClientPeer。
        public void HandleConnectedSocket(FakeSocket ws)
        {
            if (mInstance != null)
            {
                mInstance.HandleConnectedSocket(ws);
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

        public void SetName(string name) { if (mInstance != null) mInstance.SetName(name); }
        public string GetName() { return mInstance != null ? mInstance.GetName() : string.Empty; }
        public void SetID(uint id) { if (mInstance != null) mInstance.SetID(id); }
        public uint GetID() { return mInstance != null ? mInstance.GetID() : 0; }
        public void SetOwner(object owner) { if (mInstance != null) mInstance.SetOwner(owner); }
        public object GetOwner() { return mInstance != null ? mInstance.GetOwner() : null; }

        public void SendNetData(byte[] data)
        {
            if (mInstance != null) mInstance.SendNetData(data);
        }

        public void SendNetData(ReadOnlySpan<byte> data)
        {
            if (mInstance != null) mInstance.SendNetData(data);
        }
    }
}
