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
#if !UNITY_WEBGL || UNITY_EDITOR
using System.Net.WebSockets;
#endif
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AKNet.WebSocket.Client
{
    internal partial class NetClientMain : NetClientInterface, ClientPeerBase
    {
        private readonly CryptoMgr mCryptoMgr;
        private readonly ListenNetPackageMgr mPackageManager = null;
        private readonly ListenClientPeerStateMgr mListenClientPeerStateMgr = null;

        private double fReConnectServerCdTime = 0.0;
        private double fSendHeartBeatTime = 0.0;
        private double fReceiveHeartBeatTime = 0.0;

        private SOCKET_PEER_STATE mSocketPeerState;
        private SOCKET_PEER_STATE mLastSocketPeerState;
        private string mName = string.Empty;
        private uint mID = 0;
        private object mOwner = null;


        private readonly AkCircularBuffer mSendStreamList = new AkCircularBuffer();
        private readonly NetStreamCircularBuffer mReceiveStreamList = new NetStreamCircularBuffer();
        private readonly NetStreamReceivePackage mNetPackage = new NetStreamReceivePackage();
        private byte[] mSendBuffer = new byte[CommonTcpLayerConfig.nIOContexBufferLength];

#if !UNITY_WEBGL || UNITY_EDITOR
        private ClientWebSocket mWebSocket = null;
#endif
        private string ServerIp = "";
        private int nServerPort = 0;
        private IPEndPoint mIPEndPoint = null;
        private bool bReceiveTaskRunning = false;
        private bool bSending = false;
        private readonly object mWsLock = new object();

        private readonly ConfigInstance mConfigInstance;

        public NetClientMain(ConfigInstance mConfig = null)
        {
            this.mConfigInstance = mConfig ?? new ConfigInstance();

            mCryptoMgr = new CryptoMgr();
            mPackageManager = new ListenNetPackageMgr();
            mListenClientPeerStateMgr = new ListenClientPeerStateMgr();

            mSocketPeerState = mLastSocketPeerState = SOCKET_PEER_STATE.DISCONNECTED;
        }

        FrameUpdateFunc mFrameUpdateFunc = null;
        public void Update()
        {
            if(mFrameUpdateFunc == null)
            {
                mFrameUpdateFunc = new FrameUpdateFunc();
            }
            mFrameUpdateFunc.Update(Update);
        }

        public void Update(double elapsed)
        {
            if (elapsed >= 0.3)
            {
                NetLog.LogWarning("帧 时间 太长: " + elapsed);
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLServicePoll(elapsed);
#endif

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

                        double fHeatTime = Math.Min(0.3, elapsed);
                        fReceiveHeartBeatTime += fHeatTime;
                        if (fReceiveHeartBeatTime >= CommonTcpLayerConfig.fReceiveHeartBeatTimeOut)
                        {
                            fReceiveHeartBeatTime = 0.0;
                            fReConnectServerCdTime = 0.0;

                            if (mConfigInstance.bAutoReConnect)
                            {
                                SetSocketState(SOCKET_PEER_STATE.RECONNECTING);
                            }
                            else
                            {
                                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
                            }
#if DEBUG
                            NetLog.Log("心跳超时");
#endif
                        }
                    }
                    break;
                case SOCKET_PEER_STATE.RECONNECTING:
                    {
                        fReConnectServerCdTime += elapsed;
                        if (fReConnectServerCdTime >= CommonTcpLayerConfig.fReConnectMaxCdTime)
                        {
                            fReConnectServerCdTime = 0.0;
                            mSocketPeerState = SOCKET_PEER_STATE.CONNECTING;
                            ReConnectServer();
                        }
                    }
                    break;
                default:
                    break;
            }

            if (this.mSocketPeerState != this.mLastSocketPeerState)
            {
                this.mLastSocketPeerState = mSocketPeerState;
                this.mListenClientPeerStateMgr.OnSocketStateChanged(this);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SendHeartBeat() { SendNetData(CommonTcpLayerNetCommand.COMMAND_HEARTBEAT); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetSendHeartBeatTime() { fSendHeartBeatTime = 0f; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReceiveHeartBeat() { fReceiveHeartBeatTime = 0f; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetSocketState(SOCKET_PEER_STATE mSocketPeerState) { this.mSocketPeerState = mSocketPeerState; }
        public SOCKET_PEER_STATE GetSocketState() { return this.mSocketPeerState; }

        public void Reset()
        {
            SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            lock (mWsLock) { CloseSocket(); }
            lock (mSendStreamList) { mSendStreamList.Reset(); }
            lock (mReceiveStreamList) { mReceiveStreamList.Reset(); }
            fReConnectServerCdTime = 0.0f;
            fSendHeartBeatTime = 0.0;
            fReceiveHeartBeatTime = 0.0;
        }

        public void Dispose()
        {
            Reset();
            lock (mSendStreamList) { mSendStreamList.Dispose(); }
            lock (mReceiveStreamList) { mReceiveStreamList.Dispose(); }
        }

        public IPEndPoint GetIPEndPoint()
        {
            IPEndPoint mRemoteEndPoint = null;
            try { if (mIPEndPoint != null) mRemoteEndPoint = mIPEndPoint; } catch { }
            return mRemoteEndPoint;
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        private void CloseSocket()
        {
            if (mWebSocket != null)
            {
                var ws = mWebSocket;
                mWebSocket = null;
                bReceiveTaskRunning = false;
                try { ws.Dispose(); } catch { }
            }
        }
#endif

        public void addNetListenFunc(ushort nPackageId, Action<ClientPeerBase, NetPackage> fun)
        { mPackageManager.addNetListenFunc(nPackageId, fun); }
        public void removeNetListenFunc(ushort nPackageId, Action<ClientPeerBase, NetPackage> fun)
        { mPackageManager.removeNetListenFunc(nPackageId, fun); }
        public void addNetListenFunc(Action<ClientPeerBase, NetPackage> func)
        { mPackageManager.addNetListenFunc(func); }
        public void removeNetListenFunc(Action<ClientPeerBase, NetPackage> func)
        { mPackageManager.removeNetListenFunc(func); }
        public void addListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> mFunc)
        { mListenClientPeerStateMgr.addListenClientPeerStateFunc(mFunc); }
        public void removeListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> mFunc)
        { mListenClientPeerStateMgr.removeListenClientPeerStateFunc(mFunc); }
        public void addListenClientPeerStateFunc(Action<ClientPeerBase> mFunc)
        { mListenClientPeerStateMgr.addListenClientPeerStateFunc(mFunc); }
        public void removeListenClientPeerStateFunc(Action<ClientPeerBase> mFunc)
        { mListenClientPeerStateMgr.removeListenClientPeerStateFunc(mFunc); }

        public void SetName(string name) { this.mName = name; }
        public string GetName() { return this.mName; }
        public void SetID(uint id) { this.mID = id; }
        public uint GetID() { return this.mID; }
        public void SetOwner(object owner) { this.mOwner = owner; }
        public object GetOwner() { return this.mOwner; }
    }
}
