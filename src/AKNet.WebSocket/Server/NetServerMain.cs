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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AKNet.WebSocket.Server
{
    internal partial class NetServerMain : NetServerInterface
    {
        internal readonly ListenClientPeerStateMgr mListenClientPeerStateMgr = new ListenClientPeerStateMgr();
        internal readonly ListenNetPackageMgr mPackageManager = new ListenNetPackageMgr();
        internal readonly NetStreamReceivePackage mNetPackage = new NetStreamReceivePackage();
        internal readonly CryptoMgr mCryptoMgr = new CryptoMgr();

        internal readonly ClientPeerPool mClientPeerPool = null;
        private readonly List<ClientPeerWrap> mClientList = new List<ClientPeerWrap>(0);
        private readonly Queue<Socket> mConnectSocketQueue = new Queue<Socket>();

        private int nPort;
        private TcpListener mTcpListener = null;
        private CancellationTokenSource mCancellationTokenSource = new CancellationTokenSource();
        private Task mAcceptTask = null;
        private SOCKET_SERVER_STATE mState = SOCKET_SERVER_STATE.NONE;
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

        public void Dispose()
        {
            CloseNet();
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

        public int GetPort()
        {
            return this.nPort;
        }

        public SOCKET_SERVER_STATE GetServerState()
        {
            return mState;
        }

        public void Update(double elapsed)
        {
            if (elapsed >= 0.3)
            {
                NetLog.LogWarning("帧 时间 太长: " + elapsed);
            }

            while (CreateClientPeer())
            {
            }

            for (int i = mClientList.Count - 1; i >= 0; i--)
            {
                ClientPeerWrap mClientPeer = mClientList[i];
                if (mClientPeer.GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
                {
                    mClientPeer.Update(elapsed);
                }
                else
                {
                    mClientList.RemoveAt(i);
                    PrintRemoveClientMsg(mClientPeer);
                    mClientPeer.Reset();
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

        public bool MultiThreadingHandleConnectedSocket(Socket mSocket)
        {
            int nNowConnectCount = mClientList.Count + mConnectSocketQueue.Count;
            if (nNowConnectCount >= this.mConfigInstance.MaxPlayerCount)
            {
#if DEBUG
                NetLog.Log($"WebSocket 服务器爆满, 客户端总数: {nNowConnectCount}");
#endif
                return false;
            }
            else
            {
                lock (mConnectSocketQueue)
                {
                    mConnectSocketQueue.Enqueue(mSocket);
                }
                return true;
            }
        }

        private bool CreateClientPeer()
        {
            Socket mSocket = null;
            lock (mConnectSocketQueue)
            {
                mConnectSocketQueue.TryDequeue(out mSocket);
            }
            if (mSocket != null)
            {
                ClientPeerWrap clientPeer = new ClientPeerWrap(this);
                clientPeer.PerformWebSocketHandshake(mSocket);
                if (clientPeer.GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
                {
                    mClientList.Add(clientPeer);
                    PrintAddClientMsg(clientPeer);
                }
                else
                {
                    clientPeer.Reset();
                }
                return true;
            }
            return false;
        }

        private void PrintAddClientMsg(ClientPeerWrap clientPeer)
        {
#if DEBUG
            var mRemoteEndPoint = clientPeer.GetIPEndPoint();
            if (mRemoteEndPoint != null)
            {
                NetLog.Log($"WebSocket 增加客户端: {mRemoteEndPoint}, 客户端总数: {mClientList.Count}");
            }
            else
            {
                NetLog.Log($"WebSocket 增加客户端, 客户端总数: {mClientList.Count}");
            }
#endif
        }

        private void PrintRemoveClientMsg(ClientPeerWrap clientPeer)
        {
#if DEBUG
            var mRemoteEndPoint = clientPeer.GetIPEndPoint();
            if (mRemoteEndPoint != null)
            {
                NetLog.Log($"WebSocket 移除客户端: {mRemoteEndPoint}, 客户端总数: {mClientList.Count}");
            }
            else
            {
                NetLog.Log($"WebSocket 移除客户端, 客户端总数: {mClientList.Count}");
            }
#endif
        }

        public void InitNet()
        {
            List<int> mPortList = IPAddressHelper.GetAvailableTcpPortList();
            int nTryBindCount = 100;
            while (nTryBindCount-- > 0)
            {
                if (mPortList.Count > 0)
                {
                    int nPort = mPortList[RandomTool.RandomArrayIndex(0, mPortList.Count)];
                    InitNet(nPort);
                    mPortList.Remove(nPort);
                    if (GetServerState() == SOCKET_SERVER_STATE.NORMAL)
                    {
                        break;
                    }
                }
            }

            if (GetServerState() != SOCKET_SERVER_STATE.NORMAL)
            {
                NetLog.LogError("WebSocket Server 自动查找可用端口 失败！！！");
            }
        }

        public void InitNet(int nPort)
        {
            InitNet(IPAddress.Any.ToString(), nPort);
        }

        public void InitNet(string Ip, int nPort)
        {
            CloseNet();
            try
            {
                this.nPort = nPort;
                this.mBindIp = Ip;
                mState = SOCKET_SERVER_STATE.NORMAL;

                this.mTcpListener = new TcpListener(IPAddress.Parse(Ip), nPort);
                this.mTcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                this.mTcpListener.Start(this.mConfigInstance.MaxPlayerCount);

                NetLog.Log($"WebSocket 服务器 初始化成功: {Ip}:{nPort}");
                mCancellationTokenSource = new CancellationTokenSource();
                mAcceptTask = AcceptLoopAsync();
            }
            catch (Exception ex)
            {
                mState = SOCKET_SERVER_STATE.EXCEPTION;
                NetLog.LogError($"WebSocket 服务器 初始化失败: {Ip} | {nPort} | {ex.Message}");
            }
        }

        private async Task AcceptLoopAsync()
        {
            var cancellationToken = mCancellationTokenSource.Token;
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var tcpClient = await mTcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                    var mClientSocket = tcpClient.Client;

                    if (!MultiThreadingHandleConnectedSocket(mClientSocket))
                    {
                        try
                        {
                            mClientSocket.Shutdown(SocketShutdown.Both);
                        }
                        catch { }
                        finally
                        {
                            mClientSocket.Close();
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                if (mTcpListener != null)
                {
                    NetLog.LogException(e);
                }
            }
        }

        public void CloseNet()
        {
            MainThreadCheck.Check();
            mCancellationTokenSource?.Cancel();

            // 关闭所有客户端
            for (int i = mClientList.Count - 1; i >= 0; i--)
            {
                mClientList[i].Reset();
            }
            mClientList.Clear();

            lock (mConnectSocketQueue)
            {
                mConnectSocketQueue.Clear();
            }

            if (mTcpListener != null)
            {
                try
                {
                    mTcpListener.Stop();
                }
                catch { }
                mTcpListener = null;
            }

            mState = SOCKET_SERVER_STATE.NONE;
        }
    }
}
