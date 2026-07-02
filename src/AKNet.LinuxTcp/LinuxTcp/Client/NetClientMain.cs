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
using AKNet.LinuxTcp.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace AKNet.LinuxTcp.Client
{
    internal partial class NetClientMain : UdpClientPeerCommonBase, NetClientInterface, ClientPeerBase
    {
        private readonly ListenNetPackageMgr mPackageManager = new ListenNetPackageMgr();
        private readonly ListenClientPeerStateMgr mListenClientPeerStateMgr = new ListenClientPeerStateMgr();
        internal readonly UdpCheckMgr mUdpCheckPool;
        internal readonly CryptoMgr mCryptoMgr;
        private readonly ObjectPoolManager mObjectPoolManager;

        private SOCKET_PEER_STATE mSocketPeerState;
        private SOCKET_PEER_STATE mLastSocketPeerState;
        private string Name = string.Empty;
        private uint ID = 0;
        private object Owner = null;

        // LikeTCP
        private const double fConnectMaxCdTime = 2.0;
        private const double fDisConnectMaxCdTime = 2.0;
        private double fDisConnectCdTime = 0.0;
        private double fConnectCdTime = 0.0;
        private double fReceiveHeartBeatTime = 0.0;
        private double fMySendHeartBeatCdTime = 0.0;
        private double fReConnectServerCdTime = 0.0;

        // Receive
        private readonly NetStreamCircularBuffer mReceiveStreamList = new NetStreamCircularBuffer();
        private readonly NetStreamReceivePackage mNetPackage = new NetStreamReceivePackage();
        private readonly Queue<sk_buff> mWaitCheckPackageQueue = new Queue<sk_buff>();
        private readonly msghdr mTcpMsg = null;

        // Socket
        private readonly SocketAsyncEventArgs ReceiveArgs;
        private readonly SocketAsyncEventArgs SendArgs;
        private readonly AkCircularSpanBuffer mSendStreamList = new AkCircularSpanBuffer();
        private Socket mSocket = null;
        private IPEndPoint remoteEndPoint = null;
        private string ServerIp;
        private int ServerPort;
        private bool bReceiveIOContexUsed = false;
        private bool bSendIOContexUsed = false;
        private int nLastSendBytesCount = 0;

        public NetClientMain()
        {
            NetLog.Init();
            MainThreadCheck.Check();
            IPAddressHelper.GetMtu();

            mSocketPeerState = mLastSocketPeerState = SOCKET_PEER_STATE.DISCONNECTED;

            mCryptoMgr = new CryptoMgr();
            mObjectPoolManager = new ObjectPoolManager();
            mUdpCheckPool = new UdpCheckMgr(this);

            mTcpMsg = new msghdr(mReceiveStreamList, 1500);

            mSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            mSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, int.MaxValue);

            ReceiveArgs = new SocketAsyncEventArgs();
            ReceiveArgs.SetBuffer(new byte[Config.nUdpPackageFixedSize], 0, Config.nUdpPackageFixedSize);
            ReceiveArgs.Completed += ProcessReceive;

            SendArgs = new SocketAsyncEventArgs();
            SendArgs.SetBuffer(new byte[Config.nUdpPackageFixedSize], 0, Config.nUdpPackageFixedSize);
            SendArgs.Completed += ProcessSend;

            bReceiveIOContexUsed = false;
            bSendIOContexUsed = false;
        }

        public void Update(double elapsed)
        {
            if (elapsed >= 0.3)
            {
                NetLog.LogWarning("NetClient 帧 时间 太长: " + elapsed);
            }

            while (NetCheckPackageExecute())
            {

            }

            ReceiveTcpStream();

            switch (mSocketPeerState)
            {
                case SOCKET_PEER_STATE.CONNECTING:
                    {
                        fConnectCdTime += elapsed;
                        if (fConnectCdTime >= fConnectMaxCdTime)
                        {
                            ConnectServer();
                        }
                        break;
                    }
                case SOCKET_PEER_STATE.CONNECTED:
                    {
                        fMySendHeartBeatCdTime += elapsed;
                        if (fMySendHeartBeatCdTime >= Config.fMySendHeartBeatMaxTime)
                        {
                            SendHeartBeat();
                            fMySendHeartBeatCdTime = 0.0;
                        }

                        double fHeatTime = Math.Min(0.3, elapsed);
                        fReceiveHeartBeatTime += fHeatTime;
                        if (fReceiveHeartBeatTime >= Config.fReceiveHeartBeatTimeOut)
                        {
                            fReceiveHeartBeatTime = 0.0;
                            fReConnectServerCdTime = 0.0;
#if DEBUG
                            NetLog.Log("Client 接收服务器心跳 超时 ");
#endif
                            SetSocketState(SOCKET_PEER_STATE.RECONNECTING);
                        }
                        break;
                    }
                case SOCKET_PEER_STATE.DISCONNECTING:
                    {
                        fDisConnectCdTime += elapsed;
                        if (fDisConnectCdTime >= fDisConnectMaxCdTime)
                        {
                            SendDisConnect();
                        }
                        break;
                    }
                case SOCKET_PEER_STATE.DISCONNECTED:
                    break;
                case SOCKET_PEER_STATE.RECONNECTING:
                    {
                        fReConnectServerCdTime += elapsed;
                        if (fReConnectServerCdTime >= Config.fReConnectMaxCdTime)
                        {
                            fReConnectServerCdTime = 0.0;
                            ReConnectServer();
                        }
                        break;
                    }
                default:
                    break;
            }

            mUdpCheckPool.Update(elapsed);

            if (this.mSocketPeerState != this.mLastSocketPeerState)
            {
                this.mLastSocketPeerState = mSocketPeerState;
                mListenClientPeerStateMgr.OnSocketStateChanged(this);
            }
        }

        FrameUpdateFunc mFrameUpdateFunc = null;
        public void Update()
        {
            if (mFrameUpdateFunc == null)
            {
                mFrameUpdateFunc = new FrameUpdateFunc();
            }
            mFrameUpdateFunc.Update(Update);
        }

        public void SetSocketState(SOCKET_PEER_STATE mState)
        {
            this.mSocketPeerState = mState;
        }

        public SOCKET_PEER_STATE GetSocketState()
        {
            return mSocketPeerState;
        }

        public void Reset()
        {
            lock (mSendStreamList)
            {
                mSendStreamList.Reset();
            }

            lock (mWaitCheckPackageQueue)
            {
                while (mWaitCheckPackageQueue.TryDequeue(out var skb))
                {
                    skb = null;
                }
            }

            mUdpCheckPool.Reset();
            this.Name = string.Empty;
            this.ID = 0;
            this.fConnectCdTime = 0.0;
            this.fDisConnectCdTime = 0.0;
            this.fReConnectServerCdTime = 0.0;
            this.fReceiveHeartBeatTime = 0.0;
            this.fMySendHeartBeatCdTime = 0.0;
        }

        public void Dispose()
        {
            DisConnectServer();
            CloseSocket();
            mUdpCheckPool.Reset();

            lock (mWaitCheckPackageQueue)
            {
                while (mWaitCheckPackageQueue.TryDequeue(out var skb))
                {
                    skb = null;
                }
            }

            lock (mSendStreamList)
            {
                mSendStreamList.Dispose();
            }

            lock (mReceiveStreamList)
            {
                mReceiveStreamList.Dispose();
            }

            NetLog.Log("--------------- Client Release ----------------");
        }

        public void SendNetPackage(sk_buff skb)
        {
            ResetSendHeartBeatCdTime();

            MainThreadCheck.Check();
            lock (mSendStreamList)
            {
                mSendStreamList.WriteFrom(skb.GetSendBuffer());
            }

            if (!bSendIOContexUsed)
            {
                bSendIOContexUsed = true;
                SendNetStream2();
            }
        }

        public ObjectPoolManager GetObjectPoolManager()
        {
            return mObjectPoolManager;
        }

        public void NetPackageExecute(NetPackage mPackage)
        {
            mPackageManager.NetPackageExecute(this, mPackage);
        }

        public void addNetListenFunc(ushort nPackageId, Action<ClientPeerBase, NetPackage> fun)
        {
            mPackageManager.addNetListenFunc(nPackageId, fun);
        }

        public void removeNetListenFunc(ushort nPackageId, Action<ClientPeerBase, NetPackage> fun)
        {
            mPackageManager.removeNetListenFunc(nPackageId, fun);
        }

        public void addNetListenFunc(Action<ClientPeerBase, NetPackage> mFunc)
        {
            mPackageManager.addNetListenFunc(mFunc);
        }

        public void removeNetListenFunc(Action<ClientPeerBase, NetPackage> mFunc)
        {
            mPackageManager.removeNetListenFunc(mFunc);
        }

        private void OnSocketStateChanged()
        {
            mListenClientPeerStateMgr.OnSocketStateChanged(this);
        }

        public void addListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> mFunc)
        {
            mListenClientPeerStateMgr.addListenClientPeerStateFunc(mFunc);
        }

        public void removeListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> mFunc)
        {
            mListenClientPeerStateMgr.removeListenClientPeerStateFunc(mFunc);
        }

        public void addListenClientPeerStateFunc(Action<ClientPeerBase> mFunc)
        {
            mListenClientPeerStateMgr.addListenClientPeerStateFunc(mFunc);
        }

        public void removeListenClientPeerStateFunc(Action<ClientPeerBase> mFunc)
        {
            mListenClientPeerStateMgr.removeListenClientPeerStateFunc(mFunc);
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
        public void SendNetData(byte[] data) { SendNetData(0, data); }
        public void SendNetData(ReadOnlySpan<byte> data) { SendNetData(0, data); }
    }
}
