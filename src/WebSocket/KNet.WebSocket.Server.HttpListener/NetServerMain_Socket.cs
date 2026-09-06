/************************************Copyright*****************************************
 *  Project    : KNet
 *  Web        : https://github.com/1426186059/KNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/09/06 00:00:00
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using KNet.Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace KNet.WebSocket.Server
{
    // 传输层：基于系统 HttpListener 完成官方 HTTP 升级握手（零第三方依赖，天然支持 wss）。
    internal partial class NetServerMain
    {
        public void InitNet()
        {
            InitNet(IPAddress.Any.ToString(), 0);
        }

        public void InitNet(int nPort)
        {
            InitNet(IPAddress.Any.ToString(), nPort);
        }

        public void InitNet(string Ip, int nPort)
        {
            CloseNet();
            try
            {
                this.nPort = nPort;
                this.mBindIp = Ip;
                mState = SOCKET_SERVER_STATE.NORMAL;

                mListener = new System.Net.HttpListener();
                // Windows 上 HttpListener 走 http.sys：监听任意网卡用 "+" 前缀，且需要 netsh 授权：
                //   netsh http add urlacl url=http://+:{nPort}/ user=Everyone
                // Linux / 其它平台走托管实现，直接用具体 IP 或 "+" 均可。
                string host = (string.IsNullOrEmpty(Ip) || Ip == "0.0.0.0" || Ip == "*" || Ip == "Any") ? "+" : Ip;
                mListener.Prefixes.Add($"http://{host}:{nPort}/");
                mListener.Start();

                NetLog.Log($"WebSocket 服务器 初始化成功: {Ip}:{nPort}");
                mCancellationTokenSource = new CancellationTokenSource();
                // 自包含：用定时器驱动 ClientPeer.Update（心跳发送 + 超时检测/移除）
                mUpdateTimer = new Timer(_ => Update(0.1), null, 100, 100);
                _ = AcceptLoopAsync();
            }
            catch (System.Net.HttpListenerException ex) when (ex.ErrorCode == 5)
            {
                NetLog.LogError($"[HttpListener] 启动失败(拒绝访问)：请以管理员身份运行，或执行 netsh http add urlacl url=http://+:{nPort}/ user=Everyone");
                mState = SOCKET_SERVER_STATE.EXCEPTION;
            }
            catch (Exception ex)
            {
                mState = SOCKET_SERVER_STATE.EXCEPTION;
                NetLog.LogError($"WebSocket 服务器 初始化失败: {Ip} | {nPort} | {ex.Message}");
            }
        }

        private async Task AcceptLoopAsync()
        {
            var cancellationToken = mCancellationTokenSource.Token;
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var ctx = await mListener.GetContextAsync().ConfigureAwait(false);
                    if (ctx.Request.IsWebSocketRequest)
                    {
                        var wsCtx = await ctx.AcceptWebSocketAsync(null).ConfigureAwait(false);
                        var ws = wsCtx.WebSocket;
                        var ep = ctx.Request.RemoteEndPoint as IPEndPoint;

                        var wrap = new ClientPeerWrap(this);
                        wrap.AttachWebSocket(ws, ep);
                        if (!MultiThreadingHandleConnectedSocket(wrap))
                        {
                            try { ws.Dispose(); } catch { }
                            wrap.Reset();
                        }
                    }
                    else
                    {
                        ctx.Response.StatusCode = 400;
                        ctx.Response.Close();
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (System.Net.HttpListenerException) { }
            catch (Exception e)
            {
                if (mListener != null)
                {
                    NetLog.LogException(e);
                }
            }
        }
    }
}
