using System.Net;
using System.Net.WebSockets;

namespace KNet.WebSocket.Server
{
    internal struct FakeSocket
    {
        // 不再持有 HttpListenerWebSocketContext：它很重，且只在排队期间被当作“有效连接”的哨兵。
        // 改为以 mWebSocket != null 作为哨兵（见 NetServerMain_ClientPeerMgr），减少每个连接的驻留内存。
        public System.Net.WebSockets.WebSocket mWebSocket;
        public IPEndPoint mIPEndPoint;
    }
}
