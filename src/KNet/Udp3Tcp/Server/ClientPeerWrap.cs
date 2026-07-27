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
using KNet.Common;
using System;
using System.Net;
using System.Net.Sockets;

namespace KNet.Udp3Tcp.Server
{
    internal class ClientPeerWrap : ClientPeerBase
	{
        private ClientPeer mInstance = null;
        private NetServerMain mNetServer;
        public ClientPeerWrap(NetServerMain mNetServer)
		{
            this.mNetServer = mNetServer;
            this.mInstance = mNetServer.GetClientPeerPool().Pop();
            this.mInstance.SetWrap(this);
        }

        public void Reset()
        {
            if (mInstance != null)
            {
                mNetServer.GetClientPeerPool().recycle(mInstance);
                mNetServer = null;
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

		public void HandleConnectedSocket(FakeSocket mSocket)
		{
            if (mInstance != null)
            {
                mInstance.HandleConnectedSocket(mSocket);
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

        public void CloseSocket()
        {
            if (mInstance != null)
            {
                mInstance.CloseSocket();
            }
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
