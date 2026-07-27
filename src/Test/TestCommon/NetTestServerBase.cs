using KNet.Common;
using KNet.Extentions.Protobuf;
using TestCommon;
using TestProtocol;

namespace TestNetServer
{
    public abstract class NetTestServerBase
    {
        private bool bCheck_CheckIsClientPeerWrap_1 = false;
        private bool bCheck_CheckIsClientPeerWrap_2 = false;
        private bool bCheck_CheckIsClientPeerWrap_3 = false;

        NetServerMainBase mNetServer = null;
        private readonly List<ClientPeerBase> mClientPeerList = new List<ClientPeerBase>();
        const int NetCommand_COMMAND_TESTCHAT = 1000;
        const int NetCommand_COMMAND_SPACEBAR_CHAT = 1001;

        public abstract NetServerMainBase Create();

        public void Start()
        {
            NetLog.AddConsoleLog();
            Init();
            UpdateMgr.Do(Update);
        }


        public void Init()
        {
            mNetServer = Create();
            mNetServer.addNetListenFunc(NetCommand_COMMAND_TESTCHAT, ReceiveMessage);
            mNetServer.addNetListenFunc(NetCommand_COMMAND_SPACEBAR_CHAT, ReceiveSpacebarMessage);
            mNetServer.addListenClientPeerStateFunc(OnClientPeerStateChanged);
            mNetServer.InitNet(6000);
        }

        private void CheckIsClientPeerWrap(ClientPeerBase peer)
        {
            if (peer.GetType().Name != "ClientPeerWrap")
            {
                throw new InvalidOperationException($"Server期望ClientPeerWrap类型，实际收到: {peer.GetType().FullName}");
            }
        }
        
        private void OnClientPeerStateChanged(ClientPeerBase peer, SOCKET_PEER_STATE state)
        {
            if (!bCheck_CheckIsClientPeerWrap_1)
            {
                bCheck_CheckIsClientPeerWrap_1 = true;
                CheckIsClientPeerWrap(peer);
            }

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
                    mClientPeerList.RemoveAt(0);  // 先移除再Dispose，避免Dispose触发断开回调导致空列表
                    peer.Dispose();
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

        private void ReceiveMessage(ClientPeerBase peer, NetPackage mPackage)
        {
            if (!bCheck_CheckIsClientPeerWrap_2)
            {
                bCheck_CheckIsClientPeerWrap_2 = true;
                CheckIsClientPeerWrap(peer);
            }

            TESTChatMessage mdata = Proto3Tool.GetData<TESTChatMessage>(mPackage);
            peer.SendNetData(NetCommand_COMMAND_TESTCHAT, mdata);
        }

        private void ReceiveSpacebarMessage(ClientPeerBase peer, NetPackage mPackage)
        {
            if (!bCheck_CheckIsClientPeerWrap_3)
            {
                bCheck_CheckIsClientPeerWrap_3 = true;
                CheckIsClientPeerWrap(peer);
            }

            TESTChatMessage mdata = Proto3Tool.GetData<TESTChatMessage>(mPackage);
            NetLog.Log($"[服务器收到空格消息] ClientId={mdata.NClientId}, SortId={mdata.NSortId}, TalkMsg={mdata.TalkMsg}");
        }
    }
}

