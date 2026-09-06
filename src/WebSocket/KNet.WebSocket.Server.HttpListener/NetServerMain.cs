/************************************Copyright*****************************************
 *  Project    : KNet
 *  Web        : https://github.com/1426186059/KNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/09/06 00:00:00
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using KNet.Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace KNet.WebSocket.Server
{
    // 传输层无关的服务器主类（与 KNet.Tcp4 / KNet.WebSocket.Server.TcpListener 同一套书写规则）。
    // 真正完成 HTTP 升级握手的是本文件的分部 NetServerMain_Socket.cs（基于系统 HttpListener + AcceptWebSocketAsync）。
    public partial class NetServerMain : NetServerInterface
    {
        // 复用 KNet.Common 的通用管理器（与客户端/包分发一致）
        internal readonly ListenClientPeerStateMgr mListenClientPeerStateMgr = new ListenClientPeerStateMgr();
        internal readonly ListenNetPackageMgr mPackageManager = new ListenNetPackageMgr();
        internal readonly NetStreamReceivePackage mNetPackage = new NetStreamReceivePackage();
        internal readonly CryptoMgr mCryptoMgr = new CryptoMgr();

        internal readonly ClientPeerPool mClientPeerPool = null;
        private readonly List<ClientPeerWrap> mClientList = new List<ClientPeerWrap>(0);
        private readonly Queue<ClientPeerWrap> mConnectSocketQueue = new Queue<ClientPeerWrap>();

        private int nPort;
        private SOCKET_SERVER_STATE mState = SOCKET_SERVER_STATE.NONE;
        private CancellationTokenSource mCancellationTokenSource = new CancellationTokenSource();
        private System.Net.HttpListener mListener = null;
        private Timer mUpdateTimer = null;
        private readonly ConfigInstance mConfigInstance;
        private string mBindIp = null;

        public NetServerMain(ConfigInstance mConfig = null)
        {
            this.mConfigInstance = mConfig ?? new ConfigInstance();
            this.mClientPeerPool = new ClientPeerPool(this, 0, this.mConfigInstance.MaxPlayerCount);
        }

        public void OnSocketStateChanged(ClientPeerBase mClientPeer)
        {
            mListenClientPeerStateMgr.OnSocketStateChanged(mClientPeer);
        }

        public void addListenClientPeerStateFunc(Action<ClientPeerBase> mFunc)
        {
            mListenClientPeerStateMgr.addListenClientPeerStateFunc(mFunc);
        }

        public void removeListenClientPeerStateFunc(Action<ClientPeerBase> mFunc)
        {
            mListenClientPeerStateMgr.removeListenClientPeerStateFunc(mFunc);
        }

        public void addListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> mFunc)
        {
            mListenClientPeerStateMgr.addListenClientPeerStateFunc(mFunc);
        }

        public void removeListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> mFunc)
        {
            mListenClientPeerStateMgr.removeListenClientPeerStateFunc(mFunc);
        }

        public void addNetListenFunc(ushort id, Action<ClientPeerBase, NetPackage> func)
        {
            mPackageManager.addNetListenFunc(id, func);
        }

        public void removeNetListenFunc(ushort id, Action<ClientPeerBase, NetPackage> func)
        {
            mPackageManager.removeNetListenFunc(id, func);
        }

        public void addNetListenFunc(Action<ClientPeerBase, NetPackage> func)
        {
            mPackageManager.addNetListenFunc(func);
        }

        public void removeNetListenFunc(Action<ClientPeerBase, NetPackage> func)
        {
            mPackageManager.removeNetListenFunc(func);
        }

        // 满足 NetServerInterface：无参 Update 由外部主循环每帧调用，转发到带帧间隔的 Update。
        // HttpListener 自身也用内部 Timer 驱动 Update(double)，二者共用同一实现。
        public void Update()
        {
            Update(0.016);
        }

        public int GetPort() => nPort;
        public SOCKET_SERVER_STATE GetServerState() => mState;

        public void Dispose()
        {
            CloseNet();
        }

        public void CloseNet()
        {
            MainThreadCheck.Check();
            mCancellationTokenSource?.Cancel();

            for (int i = mClientList.Count - 1; i >= 0; i--)
            {
                mClientList[i].Reset();
            }
            mClientList.Clear();

            lock (mConnectSocketQueue)
            {
                while (mConnectSocketQueue.Count > 0)
                {
                    mConnectSocketQueue.Dequeue()?.Reset();
                }
            }

            if (mUpdateTimer != null)
            {
                try { mUpdateTimer.Dispose(); } catch { }
                mUpdateTimer = null;
            }

            if (mListener != null)
            {
                try { mListener.Stop(); } catch { }
                mListener = null;
            }

            mState = SOCKET_SERVER_STATE.NONE;
        }
    }
}
