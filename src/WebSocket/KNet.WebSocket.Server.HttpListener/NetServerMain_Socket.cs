// 传输层：用 BCL HttpListener 完成官方 HTTP 升级握手并提供 WebSocket 收发循环。
using KNet.Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

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
            mListener = new System.Net.HttpListener();
            mListener.Prefixes.Add($"http://127.0.0.1:{nPort}/");
            try
            {
                mListener.Start();
            }
            catch (System.Net.HttpListenerException ex) when (ex.ErrorCode == 5)
            {
                NetLog.LogError($"[HttpListener] 启动失败(拒绝访问)：请以管理员身份运行，或执行 netsh http add urlacl url=http://127.0.0.1:{nPort}/ user=Everyone");
                mState = SOCKET_SERVER_STATE.EXCEPTION;
                return;
            }
            // 事件驱动服务器没有主循环，用定时器驱动 ClientPeer.Update（心跳发送 + 超时检测/移除）
            mUpdateTimer = new Timer(_ => Update(0.1), null, 100, 100);
            _ = AcceptLoopAsync();
            NetLog.Log($"[HttpListener] WebSocket 服务器 初始化成功: 127.0.0.1:{nPort}");
        }

        private async Task AcceptLoopAsync()
        {
            try
            {
                while (!mCts.IsCancellationRequested)
                {
                    var ctx = await mListener.GetContextAsync();
                    if (ctx.Request.IsWebSocketRequest)
                    {
                        var wsCtx = await ctx.AcceptWebSocketAsync(null);
                        var ws = wsCtx.WebSocket;
                        var peer = new ClientPeer(ws, ctx.Request.RemoteEndPoint, this);
                        OnClientConnected(peer);
                        _ = RecvLoopAsync(peer, ws);
                    }
                    else
                    {
                        ctx.Response.StatusCode = 400;
                        ctx.Response.Close();
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { NetLog.LogException(e); }
        }

        private async Task RecvLoopAsync(ClientPeer peer, System.Net.WebSockets.WebSocket ws)
        {
            var buffer = new byte[8192];
            try
            {
                while (ws.State == WebSocketState.Open && !mCts.IsCancellationRequested)
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), mCts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        try { await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None); } catch { }
                        break;
                    }
                    peer.OnBinaryReceived(buffer.AsSpan(0, result.Count).ToArray());
                }
            }
            catch (Exception e)
            {
                NetLog.Log($"[HttpListener] 客户端异常: {e.Message}");
            }
            finally
            {
                OnClientDisconnected(peer);
            }
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
