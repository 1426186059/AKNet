/************************************Copyright*****************************************
 *  Project    : AKNet
 *  Web        : https://github.com/825126369/AKNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/07/02 04:26:51
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using AKNet.Common;
using AKNet.LinuxTcp.Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace AKNet.LinuxTcp.Server
{
    internal partial class ClientPeer : UdpClientPeerCommonBase, ClientPeerBase
	{
        private readonly ObjectPoolManager mObjectPoolManager;
        internal UdpCheckMgr mUdpCheckPool = null;
        private SOCKET_PEER_STATE mSocketPeerState;
        private SOCKET_PEER_STATE mLastSocketPeerState;
        private NetServerMain mNetServer;
        private ClientPeerWrap mWrap;
        private string Name = string.Empty;
        private uint ID = 0;
        private object Owner = null;

        private readonly NetStreamCircularBuffer mReceiveStreamList = null;
        private readonly msghdr mTcpMsg = null;

        // heartbeat
        private double fReceiveHeartBeatTime = 0.0;
        private double fMySendHeartBeatCdTime = 0.0;

        // socket IO
        private FakeSocket mSocket = null;
        private readonly SocketAsyncEventArgs SendArgs = new SocketAsyncEventArgs();
        private readonly AkCircularSpanBuffer mSendStreamList = null;
        private bool bSendIOContexUsed = false;
        private int nLastSendBytesCount = 0;
        private IPEndPoint mIPEndPoint;

        public ClientPeer(NetServerMain mNetServer)
        {
            this.mNetServer = mNetServer;
            mUdpCheckPool = new UdpCheckMgr(this);

            // socket
            SendArgs.Completed += ProcessSend;
            SendArgs.SetBuffer(new byte[Config.nUdpPackageFixedSize], 0, Config.nUdpPackageFixedSize);
            mSendStreamList = new AkCircularSpanBuffer();

            mObjectPoolManager = new ObjectPoolManager();
            this.mReceiveStreamList = new NetStreamCircularBuffer();
            this.mTcpMsg = new msghdr(mReceiveStreamList, 1500);
            ResetSocketState();
        }

        public void SetWrap(ClientPeerWrap mWrap)
        {
            this.mWrap = mWrap;
        }

        public void Update(double elapsed)
        {
            GetReceiveCheckPackage();
            ReceiveTcpStream();

            // heartbeat logic (from UDPLikeTCPMgr)
            var mSocketPeerState = GetSocketState();
            switch (mSocketPeerState)
            {
                case SOCKET_PEER_STATE.CONNECTED:
                    {
                        fMySendHeartBeatCdTime += elapsed;
                        if (fMySendHeartBeatCdTime >= Config.fMySendHeartBeatMaxTime)
                        {
                            fMySendHeartBeatCdTime = 0.0;
                            SendHeartBeat();
                        }

                        // 有可能网络流量大的时候，会while循环卡住
                        double fHeatTime = Math.Min(0.3, elapsed);
                        fReceiveHeartBeatTime += fHeatTime;
                        if (fReceiveHeartBeatTime >= Config.fReceiveHeartBeatTimeOut)
                        {
                            fReceiveHeartBeatTime = 0.0;
#if DEBUG
                            NetLog.Log("Server 接收服务器心跳 超时 ");
#endif
                            SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
                        }
                        break;
                    }
                default:
                    break;
            }

            mUdpCheckPool.Update(elapsed);

            OnSocketStateChanged();
        }

        public void SetSocketState(SOCKET_PEER_STATE mState)
        {
            NetLog.Assert(mState == SOCKET_PEER_STATE.CONNECTED || mState == SOCKET_PEER_STATE.DISCONNECTED);
            this.mSocketPeerState = mState;
        }

        public SOCKET_PEER_STATE GetSocketState()
		{
			return mSocketPeerState;
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnSocketStateChanged()
        {
            if (this.mSocketPeerState != this.mLastSocketPeerState)
            {
                this.mLastSocketPeerState = mSocketPeerState;
                mNetServer.OnSocketStateChanged(mWrap);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetSocketState()
        {
            this.mSocketPeerState = this.mLastSocketPeerState = SOCKET_PEER_STATE.DISCONNECTED;
        }

        private void OnConnectReset()
        {
            this.mUdpCheckPool.Reset();
            lock (mSendStreamList)
            {
                this.mSendStreamList.Reset();
            }
            this.fReceiveHeartBeatTime = 0;
            this.fMySendHeartBeatCdTime = 0;
        }

        private void OnDisConnectReset()
        {
            this.mUdpCheckPool.Reset();
            lock (mSendStreamList)
            {
                this.mSendStreamList.Reset();
            }
            this.fReceiveHeartBeatTime = 0;
            this.fMySendHeartBeatCdTime = 0;
        }

        public void Reset()
        {
            OnSocketStateChanged();
            ResetSocketState();

            CloseSocket();
            this.mUdpCheckPool.Reset();

            lock (mSendStreamList)
            {
                this.mSendStreamList.Reset();
            }

            lock (mReceiveStreamList)
            {
                mReceiveStreamList.Reset();
            }

            this.Name = string.Empty;
            this.ID = 0;
            this.fReceiveHeartBeatTime = 0;
            this.fMySendHeartBeatCdTime = 0;
            this.bSendIOContexUsed = false;
            mWrap = null;
        }

        public void Dispose()
        {
            Reset();

            SendArgs.Dispose();
            lock (mSendStreamList)
            {
                this.mSendStreamList.Dispose();
            }

            lock (mReceiveStreamList)
            {
                mReceiveStreamList.Dispose();
            }
        }

        public void SendNetPackage(sk_buff skb)
        {
            ResetSendHeartBeatCdTime();
            WriteToSendBuffer(skb.GetSendBuffer());
        }

        public ObjectPoolManager GetObjectPoolManager()
        {
            return mObjectPoolManager;
        }

        public void NetPackageExecute(NetPackage mPackage)
        {
            mNetServer.GetPackageManager().NetPackageExecute(mWrap, mPackage);
        }

        public void SetName(string name)
        {
            this.Name = name;
        }

        public string GetName()
        {
            return this.Name;
        }

        public void SetID(uint id)
        {
            this.ID = id;
        }

        public uint GetID()
        {
            return this.ID;
        }
        public void SetOwner(object owner) { this.Owner = owner; }
        public object GetOwner() { return this.Owner; }
    }
}
