using AKNet.Common;
using AKNet.Extentions.Protobuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TestCommon;
using TestProtocol;

namespace TestNetServer
{
    public abstract class QuicTestServerBase
    {
        QuicServerMainBase mNetServer = null;
        private readonly List<QuicClientPeerBase> mClientPeerList = new List<QuicClientPeerBase>();
        const int NetCommand_COMMAND_TESTCHAT = 1000;
        public const int nSingleClientStreamCount = 1;
        public abstract QuicServerMainBase Create();
        int nReceivePackageCount;

        public void Start()
        {
            NetLog.AddConsoleLog();
            Init();
            UpdateMgr.Do(Update);
        }
        
        public void Init()
        {
            mNetServer = Create();
            mNetServer.addNetListenFunc(NetCommand_COMMAND_TESTCHAT, ReceiveChatMessage);
            mNetServer.addListenClientPeerStateFunc(OnClientPeerStateChanged);
            mNetServer.InitNet(6000);
        }

        private void OnClientPeerStateChanged(QuicClientPeerBase peer, SOCKET_PEER_STATE state)
        {
            if (state == SOCKET_PEER_STATE.CONNECTED)
            {
                mClientPeerList.Add(peer);
                NetLog.Log($"客户端连接: {peer.GetIPEndPoint()}, 当前连接数: {mClientPeerList.Count}");
            }
            else if (state == SOCKET_PEER_STATE.DISCONNECTED)
            {
                mClientPeerList.Remove(peer);
            }
        }

        public void Update(double fElapsedTime)
        {
            mNetServer.Update(fElapsedTime);

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Delete)
                {
                    if (mClientPeerList.Count > 0)
                    {
                        var peer = mClientPeerList[0];
                        NetLog.Log($"Delete键按下，Dispose客户端: {peer.GetIPEndPoint()}");
                        peer.Dispose();
                        mClientPeerList.RemoveAt(0);
                    }
                    else
                    {
                        NetLog.Log("Delete键按下，但没有已连接的客户端");
                    }
                }
            }
        }

        public void Dispose()
        {
            mNetServer?.Dispose();
            mNetServer = null;
        }

        private void ReceiveChatMessage(QuicClientPeerBase peer, QuicNetPackage mPackage)
        {
            TESTChatMessage mdata = Proto3Tool.GetData<TESTChatMessage>(mPackage);
            nReceivePackageCount++;
            for (byte i = 1; i <= nSingleClientStreamCount; i++)
            {
                peer.SendNetData(i, NetCommand_COMMAND_TESTCHAT, mdata);
            }
            IMessagePool<TESTChatMessage>.recycle(mdata);

            if (nReceivePackageCount % 10000 == 0)
            {
                NetLog.Log($"接受包数量: {nReceivePackageCount}");
            }
        }
    }
}

