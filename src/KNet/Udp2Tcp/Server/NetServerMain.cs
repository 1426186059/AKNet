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
using KNet.Udp2Tcp.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace KNet.Udp2Tcp.Server
{
    internal partial class NetServerMain : NetServerInterface
	{
        private readonly NetStreamReceivePackage mLikeTcpNetPackage = new NetStreamReceivePackage();
        private readonly ListenClientPeerStateMgr mListenClientPeerStateMgr = null;
        private readonly ListenNetPackageMgr mPackageManager = null;
        private readonly ClientPeerPool mClientPeerPool = null;
        private readonly ObjectPoolManager mObjectPoolManager;
        private readonly CryptoMgr mCryptoMgr;

        private readonly Dictionary<IPEndPoint, FakeSocket> mAcceptSocketDic = null;
        private readonly FakeSocketPool mFakeSocketPool = null;

        private int nPort = 0;
        private Socket mSocket = null;
        private readonly SocketAsyncEventArgs ReceiveArgs = new SocketAsyncEventArgs();
        private SOCKET_SERVER_STATE mState = SOCKET_SERVER_STATE.NONE;
        private readonly IPEndPoint mEndPointEmpty = new IPEndPoint(IPAddress.IPv6Any, 0);
        private readonly ConfigInstance mConfigInstance;

        public NetServerMain(ConfigInstance mConfig = null)
        {
            this.mConfigInstance = mConfig ?? new ConfigInstance();

            MainThreadCheck.Check();
            mCryptoMgr = new CryptoMgr();
            mObjectPoolManager = new ObjectPoolManager();
            mPackageManager = new ListenNetPackageMgr();
            mListenClientPeerStateMgr = new ListenClientPeerStateMgr();
            mClientPeerPool = new ClientPeerPool(this, 0, this.mConfigInstance.MaxPlayerCount);

            mFakeSocketPool = new FakeSocketPool(this, this.mConfigInstance.MaxPlayerCount, this.mConfigInstance.MaxPlayerCount);
            mAcceptSocketDic = new Dictionary<IPEndPoint, FakeSocket>(this.mConfigInstance.MaxPlayerCount);

            mSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            mSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, int.MaxValue);
            ReceiveArgs.Completed += ProcessReceive;
            ReceiveArgs.SetBuffer(new byte[CommonUdpLayerConfig.nUdpPackageFixedSize], 0, CommonUdpLayerConfig.nUdpPackageFixedSize);
            ReceiveArgs.RemoteEndPoint = mEndPointEmpty;
        }

        public void Update(double elapsed)
        {
            if (elapsed >= 0.3)
            {
                NetLog.LogWarning("NetServer 帧 时间 太长: " + elapsed);
            }

            while (CreateClientPeer())
            {

            }

            for (int i = mClientList.Count - 1; i >= 0; i--)
            {
                var mClientPeer = mClientList[i];
                if (mClientPeer.GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
                {
                    mClientPeer.Update(elapsed);
                }
                else
                {
                    mClientList.RemoveAt(i);
                    PrintRemoveClientMsg(mClientPeer);
                    mClientPeer.Reset();
                    mClientPeer.CloseSocket();
                }
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

        public NetStreamReceivePackage GetLikeTcpNetPackage()
        {
            return mLikeTcpNetPackage;
        }

        public CryptoMgr GetCryptoMgr()
        {
            return mCryptoMgr;
        }

        public ListenNetPackageMgr GetPackageManager()
		{
			return mPackageManager;
		}

        public ObjectPoolManager GetObjectPoolManager()
        {
            return mObjectPoolManager;
        }

        public ClientPeerPool GetClientPeerPool()
        {
            return mClientPeerPool;
        }

        public void Dispose()
        {
            CloseSocket();
        }

        public SOCKET_SERVER_STATE GetServerState()
        {
            return mState;
        }

        public int GetPort()
        {
            return nPort;
        }

        public void addNetListenFunc(ushort id, Action<ClientPeerBase, NetPackage> mFunc)
        {
            mPackageManager.addNetListenFunc(id, mFunc);
        }

        public void removeNetListenFunc(ushort id, Action<ClientPeerBase, NetPackage> mFunc)
        {
            mPackageManager.removeNetListenFunc(id, mFunc);
        }

        public void addNetListenFunc(Action<ClientPeerBase, NetPackage> mFunc)
        {
            mPackageManager.addNetListenFunc(mFunc);
        }

        public void removeNetListenFunc(Action<ClientPeerBase, NetPackage> mFunc)
        {
            mPackageManager.removeNetListenFunc(mFunc);
        }

        public void OnSocketStateChanged(ClientPeerBase mClientPeer)
        {
            mListenClientPeerStateMgr.OnSocketStateChanged(mClientPeer);
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
    }

}