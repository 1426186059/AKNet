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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("AKNet")]
[assembly: InternalsVisibleTo("AKNet.MSQuic")]
[assembly: InternalsVisibleTo("AKNet.LinuxTcp")]
[assembly: InternalsVisibleTo("AKNet.WebSocket")]
namespace AKNet.Common
{
    internal class ListenNetPackageMgr
	{
		private readonly Dictionary<ushort, Action<ClientPeerBase, NetPackage>> mNetEventDic = null;
		private event Action<ClientPeerBase, NetPackage> mCommonListenFunc;

		public ListenNetPackageMgr()
		{
			mNetEventDic = new Dictionary<ushort, Action<ClientPeerBase, NetPackage>>();
		}

		public void NetPackageExecute(ClientPeerBase peer, NetPackage mCachePackage)
		{
			if (mCommonListenFunc != null)
			{
				mCommonListenFunc(peer, mCachePackage);
			}
			else
			{
				ushort nPackageId = mCachePackage.GetPackageId();
				if (mNetEventDic.ContainsKey(nPackageId) && mNetEventDic[nPackageId] != null)
				{
					mNetEventDic[nPackageId](peer, mCachePackage);
				}
				else
				{
					NetLog.Log("不存在的包Id: " + nPackageId);
				}
			}
		}
		
        public void addNetListenFunc(Action<ClientPeerBase, NetPackage> func)
        {
			mCommonListenFunc += func;
        }

        public void removeNetListenFunc(Action<ClientPeerBase, NetPackage> func)
        {
            mCommonListenFunc -= func;
        }

        public void addNetListenFunc(UInt16 id, Action<ClientPeerBase, NetPackage> func)
		{
			NetLog.Assert(func != null);
			if (!mNetEventDic.ContainsKey(id))
			{
				mNetEventDic[id] = func;
			}
			else
			{
				mNetEventDic[id] += func;
			}
		}

		public void removeNetListenFunc(UInt16 id, Action<ClientPeerBase, NetPackage> func)
		{
			if (mNetEventDic.ContainsKey(id))
			{
				mNetEventDic[id] -= func;
			}
		}
	}
}
