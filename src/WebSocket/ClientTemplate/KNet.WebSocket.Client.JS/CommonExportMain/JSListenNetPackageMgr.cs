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
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("KNet")]
[assembly: InternalsVisibleTo("KNet.MSQuic")]
[assembly: InternalsVisibleTo("KNet.LinuxTcp")]
[assembly: InternalsVisibleTo("KNet.WebSocket.Server")]
[assembly: InternalsVisibleTo("KNet.WebSocket.Client")]
namespace KNet.Common
{
    internal class JSListenNetPackageMgr
	{
		private readonly Dictionary<ushort, Action<JSClientPeerBase, NetPackage>> mNetEventDic = null;
		private event Action<JSClientPeerBase, NetPackage> mCommonListenFunc;

		public JSListenNetPackageMgr()
		{
			mNetEventDic = new Dictionary<ushort, Action<JSClientPeerBase, NetPackage>>();
		}

		public void NetPackageExecute(JSClientPeerBase peer, NetPackage mCachePackage)
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
		
        public void addNetListenFunc(Action<JSClientPeerBase, NetPackage> func)
        {
			mCommonListenFunc += func;
        }

        public void removeNetListenFunc(Action<JSClientPeerBase, NetPackage> func)
        {
            mCommonListenFunc -= func;
        }

        public void addNetListenFunc(UInt16 id, Action<JSClientPeerBase, NetPackage> func)
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

		public void removeNetListenFunc(UInt16 id, Action<JSClientPeerBase, NetPackage> func)
		{
			if (mNetEventDic.ContainsKey(id))
			{
				mNetEventDic[id] -= func;
			}
		}
	}
}
