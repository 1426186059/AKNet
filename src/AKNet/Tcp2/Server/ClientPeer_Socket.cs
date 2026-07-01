/************************************Copyright*****************************************
 *  Project    : AKNet
 *  Web        : https://github.com/825126369/AKNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/07/02 04:26:51
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using AKNet.Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AKNet.Tcp2.Server
{
    internal partial class ClientPeer
    {
        public void HandleConnectedTcpClient(TcpClient otherTcpClient)
        {
            MainThreadCheck.Check();
            if (bStreamsDirty)
            {
                lock (mReceiveStreamList) { mReceiveStreamList.Reset(); }
                lock (mSendStreamList) { mSendStreamList.Reset(); }
                bStreamsDirty = false;
            }
            this.mTcpClient = otherTcpClient;
            this.mNetworkStream = otherTcpClient.GetStream();
            this.mCts = new System.Threading.CancellationTokenSource();
            SetSocketState(SOCKET_PEER_STATE.CONNECTED);
            bSending = false;

            var ct = mCts.Token;
            _ = Task.Run(() => ReceiveLoop(ct));
        }

        private async void ReceiveLoop(System.Threading.CancellationToken ct)
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
                        DisConnectedWithNormal();
                        return;
                    }
                }
            }
            catch (System.OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception)
            {
                DisConnectedWithNormal();
            }
        }

        public void SendNetStream(ReadOnlySpan<byte> mBufferSegment)
        {
            ResetSendHeartBeatTime();
            lock (mSendStreamList)
            {
                mSendStreamList.WriteFrom(mBufferSegment);
            }

            if (!bSending)
            {
                bSending = true;
                var ct = mCts?.Token ?? System.Threading.CancellationToken.None;
                _ = Task.Run(() => SendLoop(ct));
            }
            else
            {
                if (!bSending && mSendStreamList.Length > 0)
                    throw new Exception("SendNetStream 有数据, 但发送不了啊");
            }
        }

        private async void SendLoop(System.Threading.CancellationToken ct)
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
            catch (System.OperationCanceledException) { bSending = false; }
            catch (ObjectDisposedException) { bSending = false; }
            catch (Exception)
            {
                bSending = false;
                DisConnectedWithNormal();
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

        private void DisConnectedWithNormal() { SetSocketState(SOCKET_PEER_STATE.DISCONNECTED); }
        private void DisConnectedWithException(Exception e) { SetSocketState(SOCKET_PEER_STATE.DISCONNECTED); }
        private void DisConnectedWithSocketError(SocketError mError) { SetSocketState(SOCKET_PEER_STATE.DISCONNECTED); }
    }
}
