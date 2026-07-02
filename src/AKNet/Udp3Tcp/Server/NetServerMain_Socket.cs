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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace AKNet.Udp3Tcp.Server
{
    internal partial class NetServerMain
    {
		public void InitNet()
		{
			List<int> mPortList = IPAddressHelper.GetAvailableUdpPortList();
			int nTryBindCount = 100;
			while (nTryBindCount-- > 0)
			{
				if (mPortList.Count > 0)
				{
					int nPort = mPortList[RandomTool.RandomArrayIndex(0, mPortList.Count)];
					InitNet(nPort);
					mPortList.Remove(nPort);
					if (GetServerState() == SOCKET_SERVER_STATE.NORMAL)
					{
						break;
					}
				}
			}

			if (GetServerState() != SOCKET_SERVER_STATE.NORMAL)
			{
				NetLog.LogError("Udp Server 自动查找可用端口 失败！！！");
			}
		}

        public void InitNet(int nPort)
        {
            InitNet(IPAddress.Any, nPort);
        }

        public void InitNet(string Ip, int nPort)
        {
            InitNet(IPAddress.Parse(Ip), nPort);
        }

		private void InitNet(IPAddress mIPAddress, int nPort)
		{
			try
			{
				mState = SOCKET_SERVER_STATE.NORMAL;
				this.nPort = nPort;

                EndPoint bindEndPoint = new IPEndPoint(mIPAddress, nPort);
				mSocket.Bind(bindEndPoint);

				NetLog.Log($"{NetType.Udp3Tcp.ToString()} 服务器 初始化成功: {bindEndPoint}");
				StartReceiveFromAsync();
			}
			catch (SocketException ex)
			{
				mState = SOCKET_SERVER_STATE.EXCEPTION;
				NetLog.LogError(ex.SocketErrorCode + " | " + ex.Message + " | " + ex.StackTrace);
				NetLog.LogError($"{NetType.Udp3Tcp.ToString()} 服务器 初始化失败: {mIPAddress} | {nPort}");
			}
			catch (Exception ex)
			{
				mState = SOCKET_SERVER_STATE.EXCEPTION;
				NetLog.LogError(ex.Message + " | " + ex.StackTrace);
				NetLog.LogError($"{NetType.Udp3Tcp.ToString()} 服务器 初始化失败: {mIPAddress} | {nPort}");
			}
		}

		public int GetPort()
		{
			return this.nPort;
		}

        public SOCKET_SERVER_STATE GetServerState()
        {
            return mState;
        }

		public Socket GetSocket()
		{
			return mSocket;
		}

		private void StartReceiveFromAsync()
		{
			while (true)
			{
				bool bIOPending = false;
				if (mSocket != null)
				{
					try
					{
						bIOPending = mSocket.ReceiveFromAsync(ReceiveArgs);
					}
					catch (Exception e)
					{
						if (mSocket != null)
						{
							NetLog.LogException(e);
						}
					}
				}

				if (bIOPending) break;

				ProcessReceive(null, ReceiveArgs);
			}
		}

		private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
		{
			ProcessReceive(sender, e);
			StartReceiveFromAsync();
		}

		private void ProcessReceive(object sender, SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
			{
				NetLog.Assert(e.RemoteEndPoint != mEndPointEmpty);
				MultiThreadingReceiveNetPackage(e);
				e.RemoteEndPoint = mEndPointEmpty;
			}
		}

		public bool SendToAsync(SocketAsyncEventArgs e)
		{			
			return mSocket.SendToAsync(e);
		}

        public void CloseSocket()
		{
            if (mSocket != null)
            {
                Socket mSocket2 = mSocket;
                mSocket = null;

                try
                {
                    mSocket2.Close();
                }
                catch (Exception) { }
            }
        }
	}

}









