/************************************Copyright*****************************************
 *  Project    : KNet
 *  Web        : https://github.com/1426186059/KNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/07/28 00:39:11
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using KNet.Common;
using KNet.MSQuic.Common;
using System.Net;
using System.Net.Security;

namespace KNet.MSQuic.Client
{
    internal partial class NetClientMain
    {
		public void ReConnectServer()
		{
            ConnectServer(this.ServerIp, this.nServerPort);
        }

		public async void ConnectServer(string ServerAddr, int ServerPort)
		{
            this.ServerIp = ServerAddr;
            this.nServerPort = ServerPort;

            CloseSocket();
            SetSocketState(SOCKET_PEER_STATE.CONNECTING);
            NetLog.Log("Client 正在连接服务器: " + this.ServerIp + " | " + this.nServerPort);

            if (mIPEndPoint == null)
			{
				IPAddress mIPAddress = IPAddress.Parse(ServerAddr);
				mIPEndPoint = new IPEndPoint(mIPAddress, ServerPort);
			}

            try
            {
                mQuicConnection = await QuicConnection.ConnectAsync(GetQuicClientConnectionOptions(mIPEndPoint)).ConfigureAwait(false);
                SetSocketState(SOCKET_PEER_STATE.CONNECTED);
                StartProcessReceive();
                NetLog.Log("Client 连接服务器成功: " + this.ServerIp + " | " + this.nServerPort);
            }
            catch (Exception e)
            {
                NetLog.LogError(e.ToString());
                SetSocketState(SOCKET_PEER_STATE.RECONNECTING);
            }
		}

        private QuicConnectionOptions GetQuicClientConnectionOptions(IPEndPoint mIPEndPoint)
        {
            var mCert = X509CertTool.GetPfxCert();
            NetLog.Assert(mCert != null, "GetCert() == null");

            var ApplicationProtocols = new List<SslApplicationProtocol>();
            ApplicationProtocols.Add(SslApplicationProtocol.Http11);
            ApplicationProtocols.Add(SslApplicationProtocol.Http2);
            ApplicationProtocols.Add(SslApplicationProtocol.Http3);

            QuicConnectionOptions mOption = new QuicConnectionOptions();
            mOption.RemoteEndPoint = mIPEndPoint;
            mOption.ClientAuthenticationOptions = new SslClientAuthenticationOptions();
            mOption.ClientAuthenticationOptions.ApplicationProtocols = ApplicationProtocols;
            mOption.ClientAuthenticationOptions.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            return mOption;
        }

        public bool DisConnectServer()
		{
			MainThreadCheck.Check();
            DisConnectServer2();
            return true;
		}

        private async void DisConnectServer2()
        {
            NetLog.Log("客户端 主动 断开服务器 Begin......");
            await mQuicConnection.CloseAsync();
            NetLog.Log("客户端 主动 断开服务器 Finish......");
        }

        public async void StartProcessReceive()
        {
            try
            {
                while (mQuicConnection != null)
                {
                    QuicStream mQuicStream = await mQuicConnection.AcceptInboundStreamAsync().ConfigureAwait(false);
                    var mStreamHandle = new ClientPeerQuicStream(this, mQuicStream);
                    mStreamHandle.StartProcessStreamReceive();
                    lock (mPendingAcceptStreamQueue)
                    {
                        mPendingAcceptStreamQueue.Enqueue(mStreamHandle);
                    }
                }
            }
            catch (Exception e)
            {
                NetLog.LogError(e.ToString());
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
                    mStreamHandle = new ClientPeerQuicStream(this, nStreamIndex);
                    mSendStreamEnumDic.Add(nStreamIndex, mStreamHandle);
                }
            }
            return mStreamHandle;
        }

        public IPEndPoint GetIPEndPoint()
        {
            if (mQuicConnection != null)
            {
                return mQuicConnection.RemoteEndPoint;
            }

            return null;
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
                    while(mPendingAcceptStreamQueue.TryDequeue(out var v))
                    {
                       
                    }
                }

                mAcceptStreamDic.Clear();
                
                QuicConnection mQuicConnection2 = mQuicConnection;
                mQuicConnection = null;
				await mQuicConnection2.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
