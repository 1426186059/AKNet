using AKNet.Common;
using AKNet.Extentions.Protobuf;
using TestCommon;
using TestProtocol;

namespace TestNetServer
{
    public abstract class NetTestServerBase
    {
        NetServerMainBase mNetServer = null;
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
            mNetServer.InitNet(6000);
        }

        public void Update(double fElapsedTime)
        {
            mNetServer.Update(fElapsedTime);
        }

        private void ReceiveMessage(ClientPeerBase peer, NetPackage mPackage)
        {
            TESTChatMessage mdata = Proto3Tool.GetData<TESTChatMessage>(mPackage);
            peer.SendNetData(NetCommand_COMMAND_TESTCHAT, mdata);
            IMessagePool<TESTChatMessage>.recycle(mdata);
        }

        private void ReceiveSpacebarMessage(ClientPeerBase peer, NetPackage mPackage)
        {
            TESTChatMessage mdata = Proto3Tool.GetData<TESTChatMessage>(mPackage);
            NetLog.Log($"[服务器收到空格消息] ClientId={mdata.NClientId}, SortId={mdata.NSortId}, TalkMsg={mdata.TalkMsg}");
            IMessagePool<TESTChatMessage>.recycle(mdata);
        }
    }
}

