// 按 KNet 服务器范式：NetServerMain : NetServerInterface，配合 ClientPeer : ClientPeerBase 与事件分发。
// 底层用 BCL HttpListener 完成官方 HTTP 升级握手（零第三方依赖，天然支持 wss）。
using KNet.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace KNet.WebSocket.Server
{
    public class NetServerMain : NetServerInterface
    {
        private int mPort;
        private SOCKET_SERVER_STATE mState = SOCKET_SERVER_STATE.NONE;
        private System.Net.HttpListener mListener = null;
        private readonly CancellationTokenSource mCts = new CancellationTokenSource();

        public int GetPort() => mPort;
        public SOCKET_SERVER_STATE GetServerState() => mState;

        private readonly Dictionary<ushort, Action<ClientPeerBase, NetPackage>> mNetEventDic = new Dictionary<ushort, Action<ClientPeerBase, NetPackage>>();
        private Action<ClientPeerBase, NetPackage> mCommonListenFunc = null;
        private Action<ClientPeerBase, SOCKET_PEER_STATE> mStateFunc1 = null;
        private Action<ClientPeerBase> mStateFunc2 = null;

        public void addNetListenFunc(Action<ClientPeerBase, NetPackage> func) { mCommonListenFunc += func; }
        public void removeNetListenFunc(Action<ClientPeerBase, NetPackage> func) { mCommonListenFunc -= func; }
        public void addNetListenFunc(ushort id, Action<ClientPeerBase, NetPackage> func)
        {
            if (!mNetEventDic.ContainsKey(id)) mNetEventDic[id] = func; else mNetEventDic[id] += func;
        }
        public void removeNetListenFunc(ushort id, Action<ClientPeerBase, NetPackage> func) { if (mNetEventDic.ContainsKey(id)) mNetEventDic[id] -= func; }
        public void addListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> func) { mStateFunc1 += func; }
        public void removeListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> func) { mStateFunc1 -= func; }
        public void addListenClientPeerStateFunc(Action<ClientPeerBase> func) { mStateFunc2 += func; }
        public void removeListenClientPeerStateFunc(Action<ClientPeerBase> func) { mStateFunc2 -= func; }

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
                        var peer = new ClientPeer(ws, ctx.Request.RemoteEndPoint);
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
                    Dispatch(peer, buffer.AsSpan(0, result.Count).ToArray());
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

        public void Update(double elapsed) { }
        public void Update() { Update(0.016); }

        public void Dispose()
        {
            mState = SOCKET_SERVER_STATE.NONE;
            try { mCts.Cancel(); } catch { }
            try { mListener?.Stop(); } catch { }
            mListener = null;
        }

        public void OnClientConnected(ClientPeer peer)
        {
            peer.SetSocketState(SOCKET_PEER_STATE.CONNECTED);
            mStateFunc2?.Invoke(peer);
            mStateFunc1?.Invoke(peer, SOCKET_PEER_STATE.CONNECTED);
            NetLog.Log($"[HttpListener] 客户端连接: {peer.GetIPEndPoint()}");
        }

        public void OnClientDisconnected(ClientPeer peer)
        {
            peer.SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            mStateFunc2?.Invoke(peer);
            mStateFunc1?.Invoke(peer, SOCKET_PEER_STATE.DISCONNECTED);
            NetLog.Log($"[HttpListener] 客户端断开: {peer.GetIPEndPoint()}");
        }

        public void Dispatch(ClientPeer peer, byte[] data)
        {
            var pkg = new NetPackageImpl(0, data);
            if (mCommonListenFunc != null) mCommonListenFunc(peer, pkg);
            else if (mNetEventDic.TryGetValue(pkg.GetPackageId(), out var func) && func != null) func(peer, pkg);
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
