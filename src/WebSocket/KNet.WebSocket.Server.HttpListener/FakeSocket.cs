using System.Net;
using System.Net.WebSockets;

namespace KNet.WebSocket.Server
{
    internal struct FakeSocket
    {
        public HttpListenerWebSocketContext mContext;
        public System.Net.WebSockets.WebSocket mWebSocket;
        public IPEndPoint mIPEndPoint;
    }
}
