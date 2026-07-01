/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库
*        Author:许珂
*        StartTime:2024/11/01 00:00:00
*        ModifyTime:2026/2/1 20:26:47
*        Copyright:MIT软件许可证
************************************Copyright*****************************************/
using AKNet.Common;
using System;
using System.Runtime.CompilerServices;

namespace AKNet.WebSocket.Server
{
    internal partial class ClientPeer : ClientPeerBase
    {
        private SOCKET_PEER_STATE mSocketPeerState;
        private SOCKET_PEER_STATE mLastSocketPeerState;
        private double fSendHeartBeatTime = 0.0;
        private double fReceiveHeartBeatTime = 0.0;

        private NetServerMain mServerMgr;
        private string mName = string.Empty;
        private uint mID = 0;
        private object mOwner = null;
        private ClientPeerWrap mWrap;


        private readonly AkCircularBuffer mSendStreamList = new AkCircularBuffer();
        private readonly NetStreamCircularBuffer mReceiveStreamList = new NetStreamCircularBuffer();
        private byte[] mSendBuffer = new byte[CommonTcpLayerConfig.nIOContexBufferLength];
        private bool bSending = false;
        public ClientPeer(NetServerMain mServerMgr)
        {
            this.mServerMgr = mServerMgr;
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

                    if (nPackageCount > 0) { ReceiveHeartBeat(); }

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
                    }
                    break;
                default:
                    break;
            }

            OnSocketStateChanged();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SendHeartBeat() { SendNetData(CommonTcpLayerNetCommand.COMMAND_HEARTBEAT); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetSendHeartBeatTime() { fSendHeartBeatTime = 0f; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReceiveHeartBeat() { fReceiveHeartBeatTime = 0.0; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetSocketState(SOCKET_PEER_STATE mState)
        {
            NetLog.Assert(mState == SOCKET_PEER_STATE.CONNECTED || mState == SOCKET_PEER_STATE.DISCONNECTED);
            this.mSocketPeerState = mState;
        }
        public SOCKET_PEER_STATE GetSocketState() { return mSocketPeerState; }

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

            CloseWebSocket();
            lock (mReceiveStreamList) { mReceiveStreamList.Reset(); }
            lock (mSendStreamList) { mSendStreamList.Reset(); }

            fSendHeartBeatTime = 0.0;
            fReceiveHeartBeatTime = 0.0;
            this.mName = string.Empty;
            this.mID = 0;
            this.mOwner = null;
        }

        public void Dispose()
        {
            Reset();

            lock (mReceiveStreamList) { mReceiveStreamList.Dispose(); }
            lock (mSendStreamList) { mSendStreamList.Dispose(); }
        }

        public void SetName(string name) { this.mName = name; }
        public string GetName() { return this.mName; }
        public void SetID(uint id) { this.mID = id; }
        public uint GetID() { return this.mID; }
        public void SetOwner(object owner) { this.mOwner = owner; }
        public object GetOwner() { return this.mOwner; }
    }
}
