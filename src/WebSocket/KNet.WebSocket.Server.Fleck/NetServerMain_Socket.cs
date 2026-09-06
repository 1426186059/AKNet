// 传输层：用 Fleck 启动 WebSocket 服务并完成连接事件接线。
using Fleck;
using KNet.Common;
using System.Net;
using System.Net.Sockets;

namespace KNet.WebSocket.Server
{
    public partial class NetServerMain : NetServerInterface
    {
        public void InitNet() { InitNet(GetFreePort()); }
        public void InitNet(int nPort) { InitNet(IPAddress.Any.ToString(), nPort); }
        public void InitNet(string Ip, int nPort)
        {
            mPort = nPort;
            mState = SOCKET_SERVER_STATE.NORMAL;
            mServer = new WebSocketServer($"ws://0.0.0.0:{nPort}");
            mServer.Start(socket =>
            {
                ClientPeer mClientPeer = new ClientPeer(socket, this);
                socket.OnOpen = () =>
                {
                    MultiThreadingHandleConnectedSocket(mClientPeer);
                };
                socket.OnClose = () => OnClientDisconnected(mClientPeer);
                // KNet 使用二进制帧承载协议包；文本帧不解析为协议包
                socket.OnBinary = bytes => mClientPeer.OnBinaryReceived(bytes);
                socket.OnMessage = _ => { };
            });
            NetLog.Log($"[Fleck] WebSocket 服务器 初始化成功: 0.0.0.0:{nPort}");
        }

        private static int GetFreePort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int p = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return p;
        }
    }
}
