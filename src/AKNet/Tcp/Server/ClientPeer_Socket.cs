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
using System.Threading;

namespace AKNet.Tcp.Server
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
			SetSocketState(SOCKET_PEER_STATE.CONNECTED);
			bSendIOContextUsed = false;
			StartReceiveEventArg();
		}

		// ---------- 接收（高频，while 循环） ----------
		private void StartReceiveEventArg()
		{
			while (mSocket != null && mSocketPeerState == SOCKET_PEER_STATE.CONNECTED)
			{
				bool bIOSyncCompleted = false;
				try
				{
					bIOSyncCompleted = !mSocket.ReceiveAsync(mReceiveIOContex);
				}
				catch (Exception e)
				{
					DisConnectedWithException(e);
					break;
				}

				if (bIOSyncCompleted) ProcessReceive(mReceiveIOContex);
				else break;
			}
		}

		private void OnIOCompleted_Receive(object sender, SocketAsyncEventArgs e)
		{
			ProcessReceive(e);
			if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
				StartReceiveEventArg();
		}

		private void ProcessReceive(SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success)
			{
				if (e.BytesTransferred > 0) { MultiThreadingReceiveSocketStream(e); }
				else { DisConnectedWithNormal(); }
			}
			else { DisConnectedWithSocketError(e.SocketError); }
		}

		// ---------- 发送（高频，while 循环） ----------
		private void StartSendEventArg()
		{
			while (mSocket != null && mSocketPeerState == SOCKET_PEER_STATE.CONNECTED)
			{
				bool bIOSyncCompleted = false;
				try { bIOSyncCompleted = !mSocket.SendAsync(mSendIOContex); }
				catch (Exception e) { bSendIOContextUsed = false; DisConnectedWithException(e); break; }

				if (bIOSyncCompleted) { if (!ProcessSendSync(mSendIOContex)) break; }
				else break;
			}
		}

		private void OnIOCompleted_Send(object sender, SocketAsyncEventArgs e)
		{
			if (ProcessSendSync(e)) StartSendEventArg();
		}

		// true=还有数据要继续发
		private bool ProcessSendSync(SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success)
			{
				if (e.BytesTransferred > 0) return SendLoopChunk(e.BytesTransferred);
				else { DisConnectedWithNormal(); bSendIOContextUsed = false; return false; }
			}
			else { DisConnectedWithSocketError(e.SocketError); bSendIOContextUsed = false; return false; }
		}

		public void SendNetStream(ReadOnlySpan<byte> mBufferSegment)
		{
			ResetSendHeartBeatTime();
			lock (mSendStreamList) { mSendStreamList.WriteFrom(mBufferSegment); }

			if (!bSendIOContextUsed)
			{
				bSendIOContextUsed = true;
				ThreadPool.QueueUserWorkItem<ValueTuple<ClientPeer, int>>(
					static state => { if (state.Item1.SendLoopChunk(state.Item2)) state.Item1.StartSendEventArg(); },
					(this, 0), false);
			}
			else
			{
				if (!bSendIOContextUsed && mSendStreamList.Length > 0)
					throw new Exception("SendNetStream 有数据, 但发送不了啊");
			}
		}

		// true=还有数据要继续发（调用方需调 StartSendEventArg 继续）
		private bool SendLoopChunk(int BytesTransferred = 0)
		{
			if (BytesTransferred > 0) { lock (mSendStreamList) { mSendStreamList.ClearBuffer(BytesTransferred); } }

			int nLength = mSendStreamList.Length;
			if (nLength > 0)
			{
				nLength = Math.Min(mSendIOContex.MemoryBuffer.Length, nLength);
				lock (mSendStreamList) { mSendStreamList.CopyTo(mSendIOContex.MemoryBuffer.Span.Slice(0, nLength)); }
				mSendIOContex.SetBuffer(0, nLength);
				return true;
			}
			else { bSendIOContextUsed = false; return false; }
		}

		public IPEndPoint GetIPEndPoint()
		{
			IPEndPoint mRemoteEndPoint = null;
			try { if (mSocket != null && mSocket.RemoteEndPoint != null) mRemoteEndPoint = mSocket.RemoteEndPoint as IPEndPoint; } catch { }
			return mRemoteEndPoint;
		}

		private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
		{
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Receive: ProcessReceive(e); StartReceiveEventArg(); break;
				case SocketAsyncOperation.Send: if (ProcessSendSync(e)) StartSendEventArg(); break;
				default: throw new ArgumentException("The last operation completed on the socket was not a receive or send");
			}
		}

		private void DisConnectedWithNormal() { SetSocketState(SOCKET_PEER_STATE.DISCONNECTED); }
		private void DisConnectedWithException(Exception e) { SetSocketState(SOCKET_PEER_STATE.DISCONNECTED); }
		private void DisConnectedWithSocketError(SocketError mError) { SetSocketState(SOCKET_PEER_STATE.DISCONNECTED); }

		void CloseSocket()
		{
			if (mSocket != null)
			{
				Socket mSocket2 = mSocket;
				mSocket = null;
				System.Threading.ThreadPool.UnsafeQueueUserWorkItem(static s => { try { ((Socket)s).Close(); } catch { } }, mSocket2);
			}
		}
	}
}
