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
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace KNet.Tcp3.Server
{
    internal partial class ClientPeer : ClientPeerBase, IPoolItemInterface
    {
        private SOCKET_PEER_STATE mSocketPeerState;
        private SOCKET_PEER_STATE mLastSocketPeerState;
        private double fSendHeartBeatTime = 0.0;
        private double fReceiveHeartBeatTime = 0.0;

        private NetServerMain mServerMgr;
        private string Name = string.Empty;
        private uint ID = 0;
        private object Owner = null;

        private readonly NetStreamCircularBuffer mReceiveStreamList = new NetStreamCircularBuffer();
        private readonly AkCircularBuffer mSendStreamList = new AkCircularBuffer();

        // APM (Begin/End) 模式
        private Socket mSocket = null;
        private bool bIsSending = false;
        private bool bIsReceiving = false;
        private bool bStreamsDirty = false;
        private readonly byte[] mReceiveBuffer;
        private readonly byte[] mSendBuffer;
        private ClientPeerWrap mWrap;

        public ClientPeer(NetServerMain mServerMgr)
        {
            this.mServerMgr = mServerMgr;

            mReceiveBuffer = new byte[CommonTcpLayerConfig.nIOContexBufferLength];
            mSendBuffer = new byte[CommonTcpLayerConfig.nIOContexBufferLength];
            bIsSending = false;
            bIsReceiving = false;

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
                        SendHeartBeat();
                        fSendHeartBeatTime = 0.0;
                    }

                    double fHeatTime = Math.Min(0.3, elapsed);
                    fReceiveHeartBeatTime += fHeatTime;
                    if (fReceiveHeartBeatTime >= CommonTcpLayerConfig.fReceiveHeartBeatTimeOut)
                    {
                        fReceiveHeartBeatTime = 0.0;
                        SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
#if DEBUG
                        NetLog.Log("心跳超时");
#endif
                    }

                    break;
                default:
                    break;
            }

            OnSocketStateChanged();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SendHeartBeat()
        {
            SendNetData(CommonTcpLayerNetCommand.COMMAND_HEARTBEAT);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetSendHeartBeatTime()
        {
            fSendHeartBeatTime = 0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReceiveHeartBeat()
        {
            fReceiveHeartBeatTime = 0.0;
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

        public void Reset()
        {
            OnSocketStateChanged();
            ResetSocketState();

            CloseSocket();
            bStreamsDirty = true;

            bIsSending = false;
            bIsReceiving = false;
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
