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
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace KNet.Tcp.Server
{
    internal partial class NetServerMain
    {
		public void InitNet()
		{
			List<int> mPortList = IPAddressHelper.GetAvailableTcpPortList();
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
				NetLog.LogError("Tcp Server 自动查找可用端口 失败！！！");
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
			CloseNet();
			try
			{
				this.nPort = nPort;
				mState = SOCKET_SERVER_STATE.NORMAL;
				IPEndPoint localEndPoint = new IPEndPoint(mIPAddress, nPort);

				this.mListenSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                this.mListenSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
                this.mListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				this.mListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, int.MaxValue);
				this.mListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, int.MaxValue);
				this.mListenSocket.Bind(localEndPoint);
				this.mListenSocket.Listen(this.mConfigInstance.MaxPlayerCount);

				NetLog.Log($"{NetType.Tcp.ToString()} 服务器 初始化成功: {localEndPoint}");
				StartAcceptEventArg();
			}
			catch (SocketException ex)
			{
				mState = SOCKET_SERVER_STATE.EXCEPTION;
				NetLog.LogError(ex.SocketErrorCode + " | " + ex.Message + " | " + ex.StackTrace);
				NetLog.LogError($"{NetType.Tcp.ToString()} 服务器 初始化失败: {mIPAddress} | {nPort}");
			}
			catch (Exception ex)
			{
				mState = SOCKET_SERVER_STATE.EXCEPTION;
				NetLog.LogError(ex.Message + " | " + ex.StackTrace);
				NetLog.LogError($"{NetType.Tcp.ToString()} 服务器 初始化失败: {mIPAddress} | {nPort}");
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

		private void StartAcceptEventArg()
		{
			bool bIOSyncCompleted = false;
			mAcceptIOContex.AcceptSocket = null;

			if (mListenSocket != null)
			{
				try
				{
					bIOSyncCompleted = !mListenSocket.AcceptAsync(mAcceptIOContex);
				}
				catch (Exception e)
				{
					if (mListenSocket != null)
					{
						NetLog.LogException(e);
					}
				}
			}
			
		if (bIOSyncCompleted)
		{
			ThreadPool.QueueUserWorkItem<ValueTuple<NetServerMain, SocketAsyncEventArgs>>(
				static state => state.Item1.ProcessAccept(state.Item2),
				(this, mAcceptIOContex), false);
		}
		}

		private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
		{
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Accept:
					this.ProcessAccept(e);
					break;
				default:
					break;
			}
		}

		private void ProcessAccept(SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success)
			{
				Socket mClientSocket = e.AcceptSocket;
#if DEBUG
				NetLog.Assert(mClientSocket != null);
#endif
				if (!MultiThreadingHandleConnectedSocket(mClientSocket))
				{
					HandleConnectFull(mClientSocket);
				}
			}
			else
			{
				NetLog.LogError("ProcessAccept: " + e.SocketError);
			}
			StartAcceptEventArg();
		}

		private void HandleConnectFull(Socket mClientSocket)
		{
			try
			{
				mClientSocket.Shutdown(SocketShutdown.Both);
			}
			catch
			{

			}
			finally
			{
				mClientSocket.Close();
			}
		}

		public void CloseNet()
		{
			MainThreadCheck.Check();
			if (mListenSocket != null)
			{
				Socket mSocket = mListenSocket;
				mListenSocket = null;
				try
				{
					mSocket.Close();
				}
				catch { }
			}

		}

	}

}