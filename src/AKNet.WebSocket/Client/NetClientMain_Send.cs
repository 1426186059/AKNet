/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库
*        Author:许珂
*        StartTime:2024/11/01 00:00:00
*        ModifyTime:2026/2/1 20:26:47
*        Copyright:MIT软件许可证
************************************Copyright*****************************************/
using AKNet.Common;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace AKNet.WebSocket.Client
{
    // 非 WebGL 实现：使用 ClientWebSocket.SendAsync
#if !UNITY_WEBGL || UNITY_EDITOR
    internal partial class NetClientMain
    {
        private bool bSendTaskRunning = false;

        internal void TryFlushSendBuffer()
        {
            if (bSendTaskRunning)
                return;

            bSendTaskRunning = true;
            Task.Run(SendLoopAsync);
        }

        private async Task SendLoopAsync()
        {
            try
            {
                while (true)
                {
                    int nLength;
                    lock (mSendStreamList)
                    {
                        nLength = mSendStreamList.Length;
                    }

                    if (nLength <= 0 || mWebSocket == null ||
                        (mWebSocket.State != WebSocketState.Open &&
                         mWebSocket.State != WebSocketState.CloseReceived))
                    {
                        break;
                    }

                    nLength = Math.Min(mSendBuffer.Length, nLength);
                    lock (mSendStreamList)
                    {
                        mSendStreamList.CopyTo(new Span<byte>(mSendBuffer, 0, nLength));
                    }

                    try
                    {
                        await mWebSocket.SendAsync(
                            new ArraySegment<byte>(mSendBuffer, 0, nLength),
                            WebSocketMessageType.Binary,
                            true,
                            mCancellationTokenSource.Token)
                            .ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        MainThreadCheck.Check();
                        NetLog.LogWarning($"WebSocket 客户端 发送异常: {e.Message}");
                        DisConnectedWithError();
                        break;
                    }

                    lock (mSendStreamList)
                    {
                        mSendStreamList.ClearBuffer(nLength);
                    }
                }
            }
            finally
            {
                bSendTaskRunning = false;

                bool hasData = false;
                lock (mSendStreamList)
                {
                    hasData = mSendStreamList.Length > 0;
                }

                if (hasData && mWebSocket != null &&
                    (mWebSocket.State == WebSocketState.Open ||
                     mWebSocket.State == WebSocketState.CloseReceived))
                {
                    bSendTaskRunning = true;
                    _ = Task.Run(SendLoopAsync);
                }
            }
        }
    }
#endif

    // WebGL 实现：使用 AKWebSocket_Send 同步发送
#if UNITY_WEBGL && !UNITY_EDITOR
    internal partial class NetClientMain
    {
        internal void TryFlushSendBuffer()
        {
            if (mWebSocketInstanceId < 0) return;

            int nLength;
            lock (mSendStreamList)
            {
                nLength = mSendStreamList.Length;
            }

            if (nLength <= 0) return;

            while (nLength > 0)
            {
                int chunkSize = Math.Min(mSendBuffer.Length, nLength);
                lock (mSendStreamList)
                {
                    mSendStreamList.CopyTo(new Span<byte>(mSendBuffer, 0, chunkSize));
                }

                int sent = AKWebSocket_Send(mWebSocketInstanceId, mSendBuffer, chunkSize);
                if (sent == 0)
                {
                    NetLog.LogWarning("WebSocket WebGL 发送失败");
                    DisConnectedWithError();
                    break;
                }

                lock (mSendStreamList)
                {
                    mSendStreamList.ClearBuffer(chunkSize);
                }

                lock (mSendStreamList)
                {
                    nLength = mSendStreamList.Length;
                }
            }
        }
    }
#endif
}
