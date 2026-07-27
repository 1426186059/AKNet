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
using KNet.Udp1Tcp.Common;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace KNet.Udp1Tcp.Server
{
    internal partial class ClientPeer : UdpClientPeerCommonBase, ClientPeerBase
	{
        internal UdpCheckMgr mUdpCheckPool = null;
        private SOCKET_PEER_STATE mSocketPeerState = SOCKET_PEER_STATE.DISCONNECTED;
        private SOCKET_PEER_STATE mLastSocketPeerState = SOCKET_PEER_STATE.DISCONNECTED;
        private NetServerMain mServerMgr;
        private string Name = string.Empty;
        private uint ID = 0;
        private object Owner = null;
        internal readonly TcpStanardRTOFunc mTcpStanardRTOFunc = new TcpStanardRTOFunc();

        private double fReceiveHeartBeatTime = 0.0;
        private double fMySendHeartBeatCdTime = 0.0;

        FakeSocket mSocket = null;
        readonly SocketAsyncEventArgs SendArgs = new SocketAsyncEventArgs();
        readonly ConcurrentQueue<NetUdpFixedSizePackage> mSendPackageQueue = null;
        readonly AkCircularManySpanBuffer mSendStreamList = null;
        bool bSendIOContexUsed = false;

        IPEndPoint mIPEndPoint;
        private ClientPeerWrap mWrap;

        public ClientPeer(NetServerMain mNetServer)
        {
            this.mServerMgr = mNetServer;
            mUdpCheckPool = new UdpCheckMgr(this);

            SendArgs.Completed += ProcessSend;
            SendArgs.SetBuffer(new byte[Config.nUdpPackageFixedSize], 0, Config.nUdpPackageFixedSize);

            if (Config.bUseSendStream)
            {
                mSendStreamList = new AkCircularManySpanBuffer(Config.nUdpPackageFixedSize);
            }
            else
            {
                mSendPackageQueue = new ConcurrentQueue<NetUdpFixedSizePackage>();
            }

            ResetSocketState();
        }

        public void SetWrap(ClientPeerWrap mWrap)
        {
            this.mWrap = mWrap;
        }

        public void Update(double elapsed)
        {
            while (GetReceivePackage())
            {

            }

            var mSocketPeerState = GetSocketState();
            switch (mSocketPeerState)
            {
                case SOCKET_PEER_STATE.CONNECTED:
                    {
                        fMySendHeartBeatCdTime += elapsed;
                        if (fMySendHeartBeatCdTime >= CommonTcpLayerConfig.fSendHeartBeatMaxTime)
                        {
                            fMySendHeartBeatCdTime = 0.0;
                            SendHeartBeat();
                        }

                        // 有可能网络流量大的时候，会while循环卡住
                        double fHeatTime = Math.Min(0.3, elapsed);
                        fReceiveHeartBeatTime += fHeatTime;
                        if (fReceiveHeartBeatTime >= CommonTcpLayerConfig.fReceiveHeartBeatTimeOut)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                mServerMgr.OnSocketStateChanged(mWrap);
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

            if (Config.bUseSendStream)
            {
                lock (mSendStreamList)
                {
                    mSendStreamList.Reset();
                }
            }
            else
            {
                while (mSendPackageQueue.TryDequeue(out var mPackage))
                {
                    mServerMgr.GetObjectPoolManager().NetUdpFixedSizePackage_Recycle(mPackage);
                }
            }

            this.fReceiveHeartBeatTime = 0;
            this.fMySendHeartBeatCdTime = 0;
        }

        private void OnDisConnectReset()
        {
            this.mUdpCheckPool.Reset();

            if (Config.bUseSendStream)
            {
                lock (mSendStreamList)
                {
                    mSendStreamList.Reset();
                }
            }
            else
            {
                while (mSendPackageQueue.TryDequeue(out var mPackage))
                {
                    mServerMgr.GetObjectPoolManager().NetUdpFixedSizePackage_Recycle(mPackage);
                }
            }

            this.fReceiveHeartBeatTime = 0;
            this.fMySendHeartBeatCdTime = 0;
        }

        public void Reset()
        {
            OnSocketStateChanged();
            ResetSocketState();

            mUdpCheckPool.Reset();

            if (Config.bUseSendStream)
            {
                lock (mSendStreamList)
                {
                    mSendStreamList.Reset();
                }
            }
            else
            {
                NetUdpFixedSizePackage mPackage = null;
                while (mSendPackageQueue.TryDequeue(out mPackage))
                {
                    mServerMgr.GetObjectPoolManager().NetUdpFixedSizePackage_Recycle(mPackage);
                }
            }

            fReceiveHeartBeatTime = 0;
            fMySendHeartBeatCdTime = 0;
            this.Name = string.Empty;
            this.ID = 0;
        }

        public void Dispose()
        {
            Reset();

            SendArgs.Dispose();
            CloseSocket();
            if (Config.bUseSendStream)
            {
                lock (mSendStreamList)
                {
                    mSendStreamList.Dispose();
                }
            }
        }

        public void SendNetPackage(NetUdpFixedSizePackage mPackage)
        {
            bool bCanSendPackage = UdpNetCommand.orInnerCommand(mPackage.nPackageId) ||
                GetSocketState() == SOCKET_PEER_STATE.CONNECTED;

            if (bCanSendPackage)
            {
                UdpStatistical.AddSendPackageCount();
                ResetSendHeartBeatCdTime();

                if (Config.bUdpCheck)
                {
                    mUdpCheckPool.SetRequestOrderId(mPackage);
                    if (UdpNetCommand.orInnerCommand(mPackage.nPackageId))
                    {
                        SendNetPackage1(mPackage);
                    }
                    else
                    {
                        UdpStatistical.AddSendCheckPackageCount();
                        mPackage.mTcpStanardRTOTimer.BeginRtt();
                        if (Config.bUseSendStream)
                        {
                            SendNetPackage1(mPackage);
                        }
                        else
                        {
                            var mCopyPackage = GetObjectPoolManager().NetUdpFixedSizePackage_Pop();
                            mCopyPackage.CopyFrom(mPackage);
                            SendNetPackage1(mCopyPackage);
                        }
                    }
                }
                else
                {
                    SendNetPackage1(mPackage);
                }
            }
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
        public ObjectPoolManager GetObjectPoolManager()
        {
            return mServerMgr.GetObjectPoolManager();
        }

        public TcpStanardRTOFunc GetTcpStanardRTOFunc()
        {
            return mTcpStanardRTOFunc;
        }

        public void NetPackageExecute(NetPackage mPackage)
        {
            mServerMgr.GetPackageManager().NetPackageExecute(mWrap, mPackage);
        }
    }
}
