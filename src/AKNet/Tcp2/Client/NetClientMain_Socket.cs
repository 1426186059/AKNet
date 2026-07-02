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
using AKNet.Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AKNet.Tcp2.Client
{
    internal partial class NetClientMain
    {
        private TcpClient mTcpClient = null;
        private NetworkStream mNetworkStream = null;
        private CancellationTokenSource mCts = null;
        private bool bSending = false;

        public void ReConnectServer()
        {
            bool Connected = false;
            try { Connected = mTcpClient != null && mTcpClient.Connected; } catch { }
            if (Connected) SetSocketState(SOCKET_PEER_STATE.CONNECTED);
            else ConnectServer(this.ServerIp, this.nServerPort);
        }

        public void ConnectServer(string ServerAddr, int ServerPort)
        {
            MainThreadCheck.Check();
            Reset();
            if (bStreamsDirty)
            {
                lock (mSendStreamList) { mSendStreamList.Reset(); }
                lock (mReceiveStreamList) { mReceiveStreamList.Reset(); }
                bStreamsDirty = false;
            }
            this.ServerIp = ServerAddr;
            this.nServerPort = ServerPort;
            SetSocketState(SOCKET_PEER_STATE.CONNECTING);

            if (mIPEndPoint == null)
            {
                IPAddress mIPAddress = IPAddress.Parse(ServerAddr);
                mIPEndPoint = new IPEndPoint(mIPAddress, ServerPort);
            }

            NetLog.Log($"Tcp2(TcpClient) 客户端 正在连接服务器: {mIPEndPoint}");

            Task.Run(async () =>
            {
                try
                {
                    mCts = new CancellationTokenSource();
                    mTcpClient = new TcpClient(AddressFamily.InterNetworkV6);
                    mTcpClient.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
                    mTcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, int.MaxValue);
                    mTcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, int.MaxValue);

                    await mTcpClient.ConnectAsync(mIPEndPoint.Address, mIPEndPoint.Port).ConfigureAwait(false);
                    mNetworkStream = mTcpClient.GetStream();

                    NetLog.Log($"Tcp2(TcpClient) 客户端 连接服务器: {mIPEndPoint} 成功");
                    SetSocketState(SOCKET_PEER_STATE.CONNECTED);

                    // 启动接收和发送循环
                    var ct = mCts.Token;
                    _ = Task.Run(() => ReceiveLoop(ct));
                }
                catch (Exception e)
                {
                    NetLog.LogError($"Tcp2(TcpClient) 客户端 连接服务器: {mIPEndPoint} 失败: {e.Message}");
                    DisConnectedWithError();
                }
            });
        }

        public bool DisConnectServer()
        {
            NetLog.Log("Tcp2(TcpClient) 客户端 主动 断开服务器 Begin......");
            MainThreadCheck.Check();

            bool Connected = false;
            try { Connected = mTcpClient != null && mTcpClient.Connected; } catch { }

            if (Connected)
            {
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTING);
                CloseSocket();
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            }
            else { SetSocketState(SOCKET_PEER_STATE.DISCONNECTED); }

            NetLog.Log("Tcp2(TcpClient) 客户端 主动 断开服务器 Finish......");
            return GetSocketState() == SOCKET_PEER_STATE.DISCONNECTED;
        }

        private async void ReceiveLoop(CancellationToken ct)
        {
            byte[] mBuffer = new byte[CommonTcpLayerConfig.nIOContexBufferLength];
            try
            {
                while (!ct.IsCancellationRequested && mNetworkStream != null)
                {
                    int nRead = await mNetworkStream.ReadAsync(mBuffer, 0, mBuffer.Length, ct).ConfigureAwait(false);
                    if (nRead > 0)
                    {
                        lock (mReceiveStreamList)
                        {
                            mReceiveStreamList.WriteFrom(new ReadOnlySpan<byte>(mBuffer, 0, nRead));
                        }
                    }
                    else
                    {
                        // nRead == 0 表示对端关闭
                        DisConnectedWithNormal();
                        return;
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception)
            {
                DisConnectedWithError();
            }
        }

        public void SendNetStream(ReadOnlySpan<byte> mBufferSegment)
        {
            ResetSendHeartBeatTime();

            // 直接写入发送缓冲区 + 触发发送
            lock (mSendStreamList)
            {
                mSendStreamList.WriteFrom(mBufferSegment);
            }

            if (!bSending)
            {
                bSending = true;
                var ct = mCts?.Token ?? CancellationToken.None;
                _ = Task.Run(() => SendLoop(ct));
            }
        }

        private async void SendLoop(CancellationToken ct)
        {
            byte[] mBuffer = new byte[CommonTcpLayerConfig.nIOContexBufferLength];
            try
            {
                while (!ct.IsCancellationRequested && mNetworkStream != null)
                {
                    int nLength;
                    lock (mSendStreamList) { nLength = mSendStreamList.Length; }

                    if (nLength <= 0)
                    {
                        bSending = false;
                        return;
                    }

                    nLength = Math.Min(mBuffer.Length, nLength);
                    lock (mSendStreamList)
                    {
                        mSendStreamList.CopyTo(new Span<byte>(mBuffer, 0, nLength));
                    }

                    await mNetworkStream.WriteAsync(mBuffer, 0, nLength, ct).ConfigureAwait(false);

                    lock (mSendStreamList)
                    {
                        mSendStreamList.ClearBuffer(nLength);
                    }
                }
            }
            catch (OperationCanceledException) { bSending = false; }
            catch (ObjectDisposedException) { bSending = false; }
            catch (Exception)
            {
                bSending = false;
                DisConnectedWithError();
            }
        }

        public IPEndPoint GetIPEndPoint()
        {
            IPEndPoint mRemoteEndPoint = null;
            try
            {
                if (mTcpClient != null && mTcpClient.Client != null && mTcpClient.Client.RemoteEndPoint != null)
                    mRemoteEndPoint = mTcpClient.Client.RemoteEndPoint as IPEndPoint;
            }
            catch { }
            return mRemoteEndPoint;
        }

        private void CloseSocket()
        {
            try { mCts?.Cancel(); } catch { }

            if (mNetworkStream != null)
            {
                try { mNetworkStream.Close(); } catch { }
                mNetworkStream = null;
            }

            if (mTcpClient != null)
            {
                try { mTcpClient.Close(); } catch { }
                mTcpClient = null;
            }

            if (mCts != null)
            {
                try { mCts.Dispose(); } catch { }
                mCts = null;
            }

            bSending = false;
        }

        private void DisConnectedWithNormal()
        {
#if DEBUG
            NetLog.Log("Tcp2(TcpClient) 客户端 正常 断开服务器 ");
#endif
            DisConnectedWithError();
        }

        private void DisConnectedWithException(Exception e)
        {
#if DEBUG
            NetLog.LogException(e);
#endif
            DisConnectedWithError();
        }

        private void DisConnectedWithSocketError(SocketError mError)
        {
#if DEBUG
            NetLog.LogError(mError);
#endif
            DisConnectedWithError();
        }

        private void DisConnectedWithError()
        {
            var mSocketPeerState = GetSocketState();
            if (mSocketPeerState == SOCKET_PEER_STATE.DISCONNECTING)
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            else if (mSocketPeerState == SOCKET_PEER_STATE.CONNECTED)
            {
                if (mConfigInstance.bAutoReConnect) SetSocketState(SOCKET_PEER_STATE.RECONNECTING);
                else SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            }
        }
    }
}
