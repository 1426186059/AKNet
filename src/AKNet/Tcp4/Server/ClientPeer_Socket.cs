/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库 - 原生Socket async/await 服务端
*        Author:许珂
*        CreateTime:2026/6/28
*        Copyright:MIT软件许可证
************************************Copyright*****************************************/
using AKNet.Common;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AKNet.Tcp4.Server
{
    internal partial class ClientPeer
    {
        public void HandleConnectedSocket(Socket otherSocket)
        {
            MainThreadCheck.Check();
            if (bStreamsDirty)
            {
                lock (mReceiveStreamList) { mReceiveStreamList.Reset(); }
                lock (mSendStreamList) { mSendStreamList.Reset(); }
                bStreamsDirty = false;
            }
            this.mSocket = otherSocket;
            this.mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, int.MaxValue);
            this.mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, int.MaxValue);
            this.mCts = new CancellationTokenSource();
            SetSocketState(SOCKET_PEER_STATE.CONNECTED);
            bSending = false;

            var ct = mCts.Token;
            _ = Task.Run(() => ReceiveLoop(ct));
        }

        private async void ReceiveLoop(CancellationToken ct)
        {
            byte[] mBuffer = new byte[CommonTcpLayerConfig.nIOContexBufferLength];
            try
            {
                while (!ct.IsCancellationRequested && mSocket != null)
                {
                    int nRead = await mSocket.ReceiveAsync(new Memory<byte>(mBuffer), SocketFlags.None, ct).ConfigureAwait(false);
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
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (SocketException)
            {
                DisConnectedWithNormal();
            }
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
                var ct = mCts?.Token ?? CancellationToken.None;
                _ = Task.Run(() => SendLoop(ct));
            }
        }

        private async void SendLoop(CancellationToken ct)
        {
            byte[] mBuffer = new byte[CommonTcpLayerConfig.nIOContexBufferLength];
            try
            {
                while (!ct.IsCancellationRequested && mSocket != null)
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

                    await mSocket.SendAsync(new ReadOnlyMemory<byte>(mBuffer, 0, nLength), SocketFlags.None, ct).ConfigureAwait(false);

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
                DisConnectedWithNormal();
            }
        }

        public IPEndPoint GetIPEndPoint()
        {
            IPEndPoint mRemoteEndPoint = null;
            try
            {
                if (mSocket != null && mSocket.RemoteEndPoint != null)
                    mRemoteEndPoint = mSocket.RemoteEndPoint as IPEndPoint;
            }
            catch { }
            return mRemoteEndPoint;
        }

        private void CloseSocket()
        {
            try { mCts?.Cancel(); } catch { }

            if (mSocket != null)
            {
                try { mSocket.Shutdown(SocketShutdown.Both); } catch { }
                try { mSocket.Close(); } catch { }
                mSocket = null;
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
