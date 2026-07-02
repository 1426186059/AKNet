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
using AKNet.MSQuic.Common;

namespace AKNet.MSQuic.Server
{
    internal partial class ServerMgr
    {
		public void Update(double elapsed)
		{
            if (elapsed >= 0.3)
            {
                NetLog.LogWarning("帧 时间 太长: " + elapsed);
            }

            while (CreateClientPeer())
			{
				
			}

			for (int i = mClientList.Count - 1; i >= 0; i--)
			{
				var mClientPeer = mClientList[i];
				if (mClientPeer.GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
				{
					mClientPeer.Update(elapsed);
				}
				else
				{
					mClientList.RemoveAt(i);
                    PrintRemoveClientMsg(mClientPeer);
                    mClientPeer.Reset();
                }
			}
		}

        FrameUpdateFunc mFrameUpdateFunc = null;
        public void Update()
        {
            if (mFrameUpdateFunc == null)
            {
                mFrameUpdateFunc = new FrameUpdateFunc();
            }
            mFrameUpdateFunc.Update(Update);
        }

        public bool MultiThreadingHandleConnectedSocket(QuicConnection connection)
		{
			int nNowConnectCount = mClientList.Count + mConnectSocketQueue.Count;
			if (nNowConnectCount >= Config.MaxPlayerCount)
			{
#if DEBUG
				NetLog.Log($"服务器爆满, 客户端总数: {nNowConnectCount}");
#endif
				return false;
			}
			else
			{
				lock (mConnectSocketQueue)
				{
					mConnectSocketQueue.Enqueue(connection);
				}
				return true;
			}
		}

		private bool CreateClientPeer()
		{
			QuicConnection connection = null;
            lock (mConnectSocketQueue)
			{
				mConnectSocketQueue.TryDequeue(out connection);
			}

			if (connection != null)
			{
				var clientPeer = new ClientPeerWrap(this);
				clientPeer.mInstance.HandleConnectedSocket(connection);
				mClientList.Add(clientPeer);
                PrintAddClientMsg(clientPeer);
				return true;
			}
			return false;
		}

        private void PrintAddClientMsg(ClientPeerWrap clientPeer)
		{
#if DEBUG
            var mRemoteEndPoint = clientPeer.GetIPEndPoint();
			if (mRemoteEndPoint != null)
			{
				NetLog.Log($"增加客户端: {mRemoteEndPoint}, 客户端总数: {mClientList.Count}");
			}
			else
			{
                NetLog.Log($"增加客户端, 客户端总数: {mClientList.Count}");
            }
#endif
        }

        private void PrintRemoveClientMsg(ClientPeerWrap clientPeer)
		{
#if DEBUG
			var mRemoteEndPoint = clientPeer.GetIPEndPoint();
			if (mRemoteEndPoint != null)
			{
				NetLog.Log($"移除客户端: {mRemoteEndPoint}, 客户端总数: {mClientList.Count}");
			}
			else
			{
                NetLog.Log($"移除客户端, 客户端总数: {mClientList.Count}");
            }
#endif
		}
	}

}