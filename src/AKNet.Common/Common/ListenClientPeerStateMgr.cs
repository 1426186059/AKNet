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
using System;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("AKNet")]
[assembly: InternalsVisibleTo("AKNet.MSQuic")]
[assembly: InternalsVisibleTo("AKNet.LinuxTcp")]
[assembly: InternalsVisibleTo("AKNet.WebSocket")]
namespace AKNet.Common
{
    internal class ListenClientPeerStateMgr
	{
		private event Action<ClientPeerBase, SOCKET_PEER_STATE> mEventFunc1;
		private event Action<ClientPeerBase> mEventFunc2;

		public void OnSocketStateChanged(ClientPeerBase mClientPeer)
		{
			MainThreadCheck.Check();
			mEventFunc2?.Invoke(mClientPeer);
			mEventFunc1?.Invoke(mClientPeer, mClientPeer.GetSocketState());
		}

        public void addListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> func)
		{
			mEventFunc1 += func;
		}

		public void removeListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> func)
		{
			mEventFunc1 -= func;
		}

		public void addListenClientPeerStateFunc(Action<ClientPeerBase> func)
		{
			mEventFunc2 += func;
		}

		public void removeListenClientPeerStateFunc(Action<ClientPeerBase> func)
		{
			mEventFunc2 -= func;
		}
	}
}
