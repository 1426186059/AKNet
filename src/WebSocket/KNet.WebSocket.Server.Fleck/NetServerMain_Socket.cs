// 传输层：用 Fleck 启动 WebSocket 服务并完成连接事件接线。
using KNet.Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Fleck;

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
                var peer = new ClientPeer(socket, this);
                socket.OnOpen = () =>
                {
                    peer.SetEndPoint(new IPEndPoint(IPAddress.Parse(socket.ConnectionInfo.ClientIpAddress), socket.ConnectionInfo.ClientPort));
                    OnClientConnected(peer);
                };
                socket.OnClose = () => OnClientDisconnected(peer);
                // KNet 使用二进制帧承载协议包；文本帧不解析为协议包
                socket.OnBinary = bytes => peer.OnBinaryReceived(bytes);
                socket.OnMessage = _ => { };
            });
            // 事件驱动服务器没有主循环，用定时器驱动 ClientPeer.Update（心跳发送 + 超时检测/移除）
            mUpdateTimer = new Timer(_ => Update(0.1), null, 100, 100);
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
