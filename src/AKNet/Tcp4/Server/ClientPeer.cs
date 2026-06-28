/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库 - 原生Socket async/await 服务端
*        Author:许珂
*        CreateTime:2026/6/28
*        Copyright:MIT软件许可证
************************************Copyright*****************************************/
using AKNet.Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AKNet.Tcp4.Server
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
        private Socket mSocket = null;
        private CancellationTokenSource mCts = null;
        private bool bSending = false;
        private bool bStreamsDirty = false;

        public ClientPeer(NetServerMain mServerMgr)
        {
            this.mServerMgr = mServerMgr;
            bSending = false;
            ResetSocketState();
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
        private void SetSocketState(SOCKET_PEER_STATE mState)
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
                mServerMgr.OnSocketStateChanged(this);
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

            bSending = false;
            fSendHeartBeatTime = 0.0;
            fReceiveHeartBeatTime = 0.0;
            this.Name = string.Empty;
            this.ID = 0;
        }

        public void Release()
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
