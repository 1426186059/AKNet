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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AKNet.Tcp.Server
{
	internal partial class ClientPeer
	{
		public void HandleConnectedSocket(Socket otherSocket)
		{
			MainThreadCheck.Check();
			this.mSocket = otherSocket;
			SetSocketState(SOCKET_PEER_STATE.CONNECTED);
			bSendIOContextUsed = false;
			StartReceiveEventArg();
		}

		// ---------- 接收（高频，while 循环） ----------
		private void StartReceiveEventArg()
		{
			while (true)
			{
				bool bIOSyncCompleted = false;
				if (mSocket != null)
				{
					try { bIOSyncCompleted = !mSocket.ReceiveAsync(mReceiveIOContex); }
					catch (Exception e) { DisConnectedWithException(e); }
				}

				if (bIOSyncCompleted) ProcessReceive(mReceiveIOContex);
				else break;
			}
		}

		private void OnIOCompleted_Receive(object sender, SocketAsyncEventArgs e)
		{
			ProcessReceive(e);
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
			while (true)
			{
				bool bIOSyncCompleted = false;
				if (mSocket != null)
				{
					try { bIOSyncCompleted = !mSocket.SendAsync(mSendIOContex); }
					catch (Exception e) { bSendIOContextUsed = false; DisConnectedWithException(e); }
				}
				else { bSendIOContextUsed = false; }

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
				Task.Run(() => { if (SendLoopChunk(0)) StartSendEventArg(); });
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
				try { mSocket2.Shutdown(SocketShutdown.Both); } catch { }
				finally { mSocket2.Close(); }
			}
		}
	}
}
