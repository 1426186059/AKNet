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
using AKNet.MSQuic.Common;
using System.Net;

namespace AKNet.MSQuic.Server
{
    internal partial class ClientPeer
    {
		public void HandleConnectedSocket(QuicConnection connection)
		{
			MainThreadCheck.Check();
			this.mQuicConnection = connection;
            this.SetSocketState(SOCKET_PEER_STATE.CONNECTED);
            this.StartProcessReceive();
		}

        public IPEndPoint GetIPEndPoint()
        {
            if (mQuicConnection != null)
            {
                return mQuicConnection.RemoteEndPoint;
            }
            return null;
        }

        private async void StartProcessReceive()
		{
            try
			{
				while (mQuicConnection != null)
				{
					QuicStream mQuicStream = await mQuicConnection.AcceptInboundStreamAsync().ConfigureAwait(false);
                    var mStreamHandle = new ClientPeerQuicStream(this.mServerMgr, this, mQuicStream);
                    mStreamHandle.StartProcessStreamReceive();
                    lock (mPendingAcceptStreamQueue)
                    {
                        mPendingAcceptStreamQueue.Enqueue(mStreamHandle);
                    }
                }
			}
			catch (Exception e)
			{
				//NetLog.LogError(e.ToString());
				SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
			}
        }

        private ClientPeerQuicStream GetOrCreateSendStreamHandle(byte nStreamIndex)
        {
            ClientPeerQuicStream mStreamHandle = null;
            lock (mSendStreamEnumDic)
            {
                if (!mSendStreamEnumDic.TryGetValue(nStreamIndex, out mStreamHandle))
                {
                    mStreamHandle = new ClientPeerQuicStream(this.mServerMgr, this, nStreamIndex);
                    mSendStreamEnumDic.Add(nStreamIndex, mStreamHandle);
                }
            }
            return mStreamHandle;
        }

        private async void CloseSocket()
		{
            if (mQuicConnection != null)
            {
                lock (mSendStreamEnumDic)
                {
                    mSendStreamEnumDic.Clear();
                }

                lock (mPendingAcceptStreamQueue)
                {
                    while (mPendingAcceptStreamQueue.TryDequeue(out var v))
                    {
                        
                    }
                }

                mAcceptStreamDic.Clear();
                
                var mQuicConnection2 = mQuicConnection;
                mQuicConnection = null;
                await mQuicConnection2.CloseAsync().ConfigureAwait(false);
            }
		}
    }

}