/************************************Copyright*****************************************
 *  Project    : AKNet
 *  Web        : https://github.com/1426186059/AKNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/07/02 18:05:45
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
#if UNITY_WEBGL && !UNITY_EDITOR
using AKNet.Common;
using System;
using System.Runtime.InteropServices;

namespace AKNet.WebSocket.Client
{
    internal partial class NetClientMain
    {
        #region JS Interop (AKNet.WebSocket.jslib)

        [DllImport("__Internal")]
        private static extern int AKWebSocket_Connect(string url, int instanceId);

        [DllImport("__Internal")]
        private static extern int AKWebSocket_Close(int instanceId);

        [DllImport("__Internal")]
        private static extern int AKWebSocket_Send(int instanceId, byte[] data, int dataLen);

        [DllImport("__Internal")]
        private static extern int AKWebSocket_GetReadyState(int instanceId);

        [DllImport("__Internal")]
        private static extern int AKWebSocket_PollEvent(int instanceId);

        [DllImport("__Internal")]
        private static extern int AKWebSocket_PollMessageLength(int instanceId);

        [DllImport("__Internal")]
        private static extern int AKWebSocket_PollMessageCopy(int instanceId, byte[] destBuffer, int maxLen);

        #endregion

        #region WebGL Fields

        private int mWebGLInstanceId;
        private static int s_NextInstanceId = 1;
        private byte[] mWebGLRecvBuffer = new byte[1024 * 64];
        private byte[] mWebGLSendBuffer = new byte[1024 * 64];
        private double mWebGLReconnectCd = 0.0;

        #endregion

        #region WebGL Socket Operations

        public void ConnectServer(string ServerAddr, int ServerPort)
        {
            MainThreadCheck.Check();
            Reset();

            this.ServerIp = ServerAddr;
            this.nServerPort = ServerPort;
            SetSocketState(SOCKET_PEER_STATE.CONNECTING);

            mWebGLInstanceId = s_NextInstanceId++;
            var uri = $"ws://{ServerAddr}:{ServerPort}/";

            NetLog.Log($"WebSocket(WebGL) 正在连接服务器: {uri}, instanceId={mWebGLInstanceId}");

            int result = AKWebSocket_Connect(uri, mWebGLInstanceId);
            if (result == 0)
            {
                NetLog.LogError($"WebSocket(WebGL) 连接服务器失败: {uri}");
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            }
        }

        public void ReConnectServer()
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
                return;

            mWebGLReconnectCd = 0.0;
            ConnectServer(this.ServerIp, this.nServerPort);
        }

        public bool DisConnectServer()
        {
            NetLog.Log("WebSocket(WebGL) 主动断开服务器 Begin......");
            MainThreadCheck.Check();

            if (mWebGLInstanceId > 0)
            {
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTING);
                AKWebSocket_Close(mWebGLInstanceId);
            }

            SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            NetLog.Log("WebSocket(WebGL) 主动断开服务器 Finish......");
            return true;
        }

        private void SendNetStream(ReadOnlySpan<byte> mBufferSegment)
        {
            ResetSendHeartBeatTime();

            if (mBufferSegment.Length > mWebGLSendBuffer.Length)
            {
                NetLog.LogError($"SendNetStream(WebGL) 数据过大: {mBufferSegment.Length} > {mWebGLSendBuffer.Length}");
                return;
            }

            mBufferSegment.CopyTo(mWebGLSendBuffer);
            int result = AKWebSocket_Send(mWebGLInstanceId, mWebGLSendBuffer, mBufferSegment.Length);

            if (result == 0)
            {
                NetLog.LogWarning("WebSocket(WebGL) 发送失败，连接可能已断开");
                DisConnectedWithError();
            }
        }

        private void CloseSocket()
        {
            if (mWebGLInstanceId > 0)
            {
                AKWebSocket_Close(mWebGLInstanceId);
                mWebGLInstanceId = 0;
            }
        }

        private void DisConnectedWithError()
        {
            if (GetSocketState() == SOCKET_PEER_STATE.DISCONNECTING)
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

        #region WebGL Update Polling

        internal void WebGLServicePoll(double elapsed)
        {
            if (mWebGLInstanceId <= 0)
                return;

            // 1. 轮询连接事件
            PollWebGLEvents();

            // 2. 轮询接收数据
            PollWebGLMessages();
        }

        private void PollWebGLEvents()
        {
            while (true)
            {
                int evt = AKWebSocket_PollEvent(mWebGLInstanceId);
                if (evt == 0) break; // 无事件

                switch (evt)
                {
                    case 1: // Opened
                        NetLog.Log($"WebSocket(WebGL) 连接成功, instanceId={mWebGLInstanceId}");
                        MainThreadCheck.Check();
                        SetSocketState(SOCKET_PEER_STATE.CONNECTED);
                        break;

                    case 2: // Closed
                        NetLog.Log($"WebSocket(WebGL) 连接关闭, instanceId={mWebGLInstanceId}");
                        MainThreadCheck.Check();
                        DisConnectedWithError();
                        break;

                    case 3: // Error
                        NetLog.LogError($"WebSocket(WebGL) 连接错误, instanceId={mWebGLInstanceId}");
                        MainThreadCheck.Check();
                        DisConnectedWithError();
                        break;
                }
            }
        }

        private void PollWebGLMessages()
        {
            while (true)
            {
                int msgLen = AKWebSocket_PollMessageLength(mWebGLInstanceId);
                if (msgLen <= 0) break;

                if (msgLen > mWebGLRecvBuffer.Length)
                {
                    NetLog.LogError($"WebSocket(WebGL) 接收消息过大: {msgLen} > {mWebGLRecvBuffer.Length}");
                    // 丢弃这条消息
                    AKWebSocket_PollMessageCopy(mWebGLInstanceId, mWebGLRecvBuffer, 0);
                    continue;
                }

                int copied = AKWebSocket_PollMessageCopy(mWebGLInstanceId, mWebGLRecvBuffer, msgLen);
                if (copied > 0)
                {
                    lock (mReceiveStreamList)
                    {
                        mReceiveStreamList.WriteFrom(
                            new ReadOnlySpan<byte>(mWebGLRecvBuffer, 0, copied));
                    }
                }
            }
        }

        #endregion
    }
}
#endif
