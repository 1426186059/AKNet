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
using AKNet.Udp4Tcp.Common;
using System;
using System.Runtime.CompilerServices;

namespace AKNet.Udp4Tcp.Server
{
    internal partial class ClientPeer : ClientPeerBase
	{
        private SOCKET_PEER_STATE mSocketPeerState;
        private SOCKET_PEER_STATE mLastSocketPeerState;
        private NetServerMain mServerMgr;

        private string Name = string.Empty;
        private uint ID = 0;
        private object Owner = null;
                       private double fReceiveHeartBeatTime = 0.0;
        private double fSendHeartBeatTime = 0.0;

        private readonly AkCircularManyBuffer mSendStreamList = new AkCircularManyBuffer();
        private readonly NetStreamCircularBuffer mReceiveStreamList = new NetStreamCircularBuffer();
        private Connection mConnection = null;

        private readonly Memory<byte> ReceiveArgs = new byte[Config.nUdpPackageFixedSize];
        private readonly Memory<byte> SendArgs = new byte[Config.nUdpPackageFixedSize];
        private bool bSendIOContexUsed = false;
        private ClientPeerWrap mWrap;

        public ClientPeer(NetServerMain mNetServer)
        {
            this.mServerMgr = mNetServer;
            bSendIOContexUsed = false;
            ResetSocketState();
        }

        public void SetWrap(ClientPeerWrap mWrap)
        {
            this.mWrap = mWrap;
        }

        public void Update(double elapsed)
        {
            switch (mSocketPeerState)
            {
                case SOCKET_PEER_STATE.CONNECTED:
                    {
                        int nPackageCount = 0;
                        while (NetPackageExecute())
                        {
                            nPackageCount++;
                        }
                        if (nPackageCount > 0)
                        {
                            ReceiveHeartBeat();
                        }

                        fSendHeartBeatTime += elapsed;
                        if (fSendHeartBeatTime >= CommonTcpLayerConfig.fSendHeartBeatMaxTime)
                        {
                            fSendHeartBeatTime = 0.0;
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

            OnSocketStateChanged();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetSocketState(SOCKET_PEER_STATE mState)
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SendHeartBeat()
        {
            SendNetData(CommonTcpLayerNetCommand.COMMAND_HEARTBEAT);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetSendHeartBeatCdTime()
        {
            fSendHeartBeatTime = 0.0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReceiveHeartBeat()
        {
            fReceiveHeartBeatTime = 0.0;
        }

        public void Reset()
        {
            OnSocketStateChanged();
            ResetSocketState();

            CloseSocket();
            lock (mReceiveStreamList)
            {
                mReceiveStreamList.Reset();
            }

            lock (mSendStreamList)
            {
                mSendStreamList.Reset();
            }

            bSendIOContexUsed = false;
            fSendHeartBeatTime = 0.0;
            fReceiveHeartBeatTime = 0.0;
            this.Name = string.Empty;
            this.ID = 0;
            mWrap = null;
        }

        public void Dispose()
        {
            Reset();

            lock (mReceiveStreamList)
            {
                mReceiveStreamList.Dispose();
            }

            lock (mSendStreamList)
            {
                mSendStreamList.Dispose();
            }
        }

        public void NetPackageExecute(NetPackage mPackage)
        {
            mServerMgr.GetPackageManager().NetPackageExecute(mWrap, mPackage);
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

