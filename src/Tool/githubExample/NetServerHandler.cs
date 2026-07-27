using KNet.Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using TestProtocol;

namespace githubExample
{
    public class NetServerHandler
    {
        NetServerMain mNetServer = null;
        private readonly List<ClientPeerBase> mClientPeerList = new List<ClientPeerBase>();
        const int COMMAND_TESTCHAT = 1000;
        public void Init()
        {
            ConfigInstance mConfig = new ConfigInstance();
            mConfig.bAutoReConnect = false;
            mConfig.MaxPlayerCount = 10;
            mNetServer = new NetServerMain(NetType.Udp3Tcp, mConfig);
            mNetServer.addNetListenFunc(COMMAND_TESTCHAT, receive_csChat);
            mNetServer.addListenClientPeerStateFunc(OnClientPeerStateChanged);
            mNetServer.InitNet(6000);
        }

        private void OnClientPeerStateChanged(ClientPeerBase peer, SOCKET_PEER_STATE state)
        {
            if (state == SOCKET_PEER_STATE.CONNECTED)
            {
                mClientPeerList.Add(peer);
                Console.WriteLine($"客户端连接: {peer.GetIPEndPoint()}, 当前连接数: {mClientPeerList.Count}");
            }
            else if (state == SOCKET_PEER_STATE.DISCONNECTED)
            {
                mClientPeerList.Remove(peer);
            }
        }

        public void Update()
        {
            mNetServer.Update();

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Delete)
                {
                    if (mClientPeerList.Count > 0)
                    {
                        var peer = mClientPeerList[0];
                        Console.WriteLine($"Delete键按下，Dispose客户端: {peer.GetIPEndPoint()}");
                        peer.Dispose();
                        mClientPeerList.RemoveAt(0);
                    }
                    else
                    {
                        Console.WriteLine("Delete键按下，但没有已连接的客户端");
                    }
                }
            }
        }

        public void Dispose()
        {
            mNetServer?.Dispose();
            mNetServer = null;
        }

        private static void receive_csChat(ClientPeerBase clientPeer, NetPackage package)
        {
            TESTChatMessage mReceiveMsg = TESTChatMessage.Parser.ParseFrom(package.GetData());
            Console.WriteLine(mReceiveMsg.TalkMsg);
            SendMsg(clientPeer);
        }

        private static void SendMsg(ClientPeerBase peer)
        {
            TESTChatMessage mdata = new TESTChatMessage();
            mdata.TalkMsg = "Hello, KNet Client";
            peer.SendNetData(COMMAND_TESTCHAT, mdata.ToByteArray());
        }
    }
}