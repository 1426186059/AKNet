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
using System.Runtime.InteropServices.JavaScript;

namespace KNet.WebSocket.Client
{
    /// <summary>
    /// V2 浏览器 WebSocket 后端。JS 只提供 WebSocket 原语，C# 保留全套 KNet 逻辑。
    /// 轮询模型（移植自 Web_Mir3 JSBind/BrowserWebSocket.cs + jsengine/core/websocket.js）：
    /// JS 把二进制帧入队，C# 每帧 BrowserServicePoll 取出写入接收环形缓冲，
    /// 再由 NetClientMain 的 NetPackageExecute() 经 CryptoMgr 解帧分发给监听器。
    /// </summary>
    public partial class NetClientMain
    {
        #region JS Interop (对应 wwwroot/jsengine/knet-websocket.js 的 knet.ws.*)

        [JSImport("knet.wsConnect", "main.js")]
        private static partial int WsConnect(string url);

        [JSImport("knet.wsClose", "main.js")]
        private static partial void WsClose(int instanceId);

        // 拷贝发送：支持子区间（offset/length），调用方可直接指定要发的字节范围，无需每次 new 整个数组。
        [JSImport("knet.wsSend", "main.js")]
        private static partial int WsSend(int instanceId, byte[] data, int offset, int length);

        // 指针发送（零拷贝）：C# 以 MemoryView 把“wasm 线性内存指针 + 长度”直接交给 JS。
        // MemoryView 在 JS 侧表现为 { buffer(=wasm 堆 ArrayBuffer), byteOffset(=指针), byteLength(=长度) }，
        // JS 取出 view.byteOffset 作为指针、view.byteLength 作为长度，在 wasm 堆上建 Uint8Array 视图后 ws.send，
        // 避免 byte[] → Uint8Array 的 marshalling 拷贝。
        // 注：此 .NET wasm 运行时未暴露 getMemory，裸 IntPtr 无法被 JS 读取；MemoryView 是唯一可用的
        // “传指针+长度”机制，其 byteOffset 即等价于 C# 侧的指针。
        [JSImport("knet.wsSendPointer", "main.js")]
        private static partial int WsSendPointer(int instanceId, [JSMarshalAs<JSType.MemoryView>] Span<byte> data);

        [JSImport("knet.wsGetState", "main.js")]
        private static partial int WsGetState(int instanceId);

        [JSImport("knet.wsReceive", "main.js")]
        private static partial byte[] WsReceive(int instanceId);

        #endregion

        #region Browser Fields

        private int mInstanceId = -1;
        private bool mZeroCopySend;

        #endregion

        #region Socket Operations

        public void ReConnectServer()
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
                return;
            ConnectServer(this.ServerIp, this.nServerPort);
        }

        public void ConnectServer(string ServerAddr, int ServerPort)
        {
            MainThreadCheck.Check();
            Reset();

            this.ServerIp = ServerAddr;
            this.nServerPort = ServerPort;
            SetSocketState(SOCKET_PEER_STATE.CONNECTING);

            var uri = $"ws://{ServerAddr}:{ServerPort}/";
            NetLog.Log($"WebSocket(V2) 正在连接服务器: {uri}");

            mInstanceId = WsConnect(uri);
            if (mInstanceId <= 0)
            {
                NetLog.LogError($"WebSocket(V2) 连接服务器失败: {uri}");
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            }
        }

        public bool DisConnectServer()
        {
            NetLog.Log("WebSocket(V2) 主动断开服务器 Begin......");
            MainThreadCheck.Check();

            if (mInstanceId > 0)
            {
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTING);
                WsClose(mInstanceId);
                mInstanceId = -1;
            }

            SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            NetLog.Log("WebSocket(V2) 主动断开服务器 Finish......");
            return true;
        }

        private unsafe void SendNetStream(ArraySegment<byte> mBufferSegment)
        {
            ResetSendHeartBeatTime();

            if (mInstanceId <= 0 || mBufferSegment.Count == 0)
                return;

            int result;
            if (mZeroCopySend)
            {
                result = WsSendPointer(mInstanceId, mBufferSegment.AsSpan());   // 零拷贝：byte[] → Span<byte> 视图（指针+长度），不拷贝
            }
            else
            {
                result = WsSend(mInstanceId, mBufferSegment.Array, 0, mBufferSegment.Count); // 拷贝：子区间发送（offset=0, length=全长）
            }

            if (result == 0)
            {
                NetLog.LogWarning("WebSocket(V2) 发送失败，连接可能已断开");
                DisConnectedWithError();
            }
        }

        private void CloseSocket()
        {
            if (mInstanceId > 0)
            {
                WsClose(mInstanceId);
                mInstanceId = -1;
            }
        }

        private void DisConnectedWithError()
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTING)
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            else if (GetSocketState() == SOCKET_PEER_STATE.DISCONNECTING)
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            else if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
            {
                if (mConfigInstance.bAutoReConnect)
                    SetSocketState(SOCKET_PEER_STATE.RECONNECTING);
                else
                    SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            }
        }

        #endregion

        #region Browser Polling

        /// <summary>每帧轮询：连接状态 + 接收消息。对应 Unity WebGL 的 WebGLServicePoll。</summary>
        internal void BrowserServicePoll(double elapsed)
        {
            if (mInstanceId <= 0)
                return;

            int state = WsGetState(mInstanceId);
            // readyState: 0 CONNECTING, 1 OPEN, 2 CLOSING, 3 CLOSED
            SOCKET_PEER_STATE peerState = GetSocketState();

            if (state == 1 && peerState != SOCKET_PEER_STATE.CONNECTED)
            {
                NetLog.Log($"WebSocket(V2) 连接成功, state={state}");
                MainThreadCheck.Check();
                SetSocketState(SOCKET_PEER_STATE.CONNECTED);
            }
            else if (state >= 2 && peerState != SOCKET_PEER_STATE.DISCONNECTED &&
                     peerState != SOCKET_PEER_STATE.DISCONNECTING &&
                     peerState != SOCKET_PEER_STATE.RECONNECTING)
            {
                NetLog.Log($"WebSocket(V2) 连接关闭/错误, state={state}");
                MainThreadCheck.Check();
                DisConnectedWithError();
            }

            // 轮询接收：一条 WS 消息 = 一段字节流，拷入接收环形缓冲
            while (true)
            {
                byte[] msg = WsReceive(mInstanceId);
                if (msg == null || msg.Length == 0) break;

                lock (mReceiveStreamList)
                {
                    mReceiveStreamList.WriteFrom(new ReadOnlySpan<byte>(msg));
                }
            }
        }

        #endregion
    }
}
