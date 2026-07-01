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
namespace AKNet.Common
{
    internal class QuicListenNetPackageMgr
	{
		private readonly Dictionary<ushort, Action<QuicClientPeerBase, QuicNetPackage>> mNetEventDic = null;
		private event Action<QuicClientPeerBase, QuicNetPackage> mCommonListenFunc = null;

		public QuicListenNetPackageMgr()
		{
			mNetEventDic = new Dictionary<ushort, Action<QuicClientPeerBase, QuicNetPackage>>();
		}

		public void NetPackageExecute(QuicClientPeerBase peer, QuicNetPackage mPackage)
		{
			if (mCommonListenFunc != null)
			{
				mCommonListenFunc(peer, mPackage);
			}
			else
			{
				ushort nPackageId = mPackage.GetPackageId();
				if (mNetEventDic.ContainsKey(nPackageId) && mNetEventDic[nPackageId] != null)
				{
					mNetEventDic[nPackageId](peer, mPackage);
				}
				else
				{
					NetLog.Log("不存在的包Id: " + nPackageId);
				}
			}
		}
		
        public void addNetListenFunc(Action<QuicClientPeerBase, QuicNetPackage> func)
        {
			mCommonListenFunc += func;
        }

        public void removeNetListenFunc(Action<QuicClientPeerBase, QuicNetPackage> func)
        {
            mCommonListenFunc -= func;
        }

        public void addNetListenFunc(UInt16 id, Action<QuicClientPeerBase, QuicNetPackage> func)
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

		public void removeNetListenFunc(UInt16 id, Action<QuicClientPeerBase, QuicNetPackage> func)
		{
			if (mNetEventDic.ContainsKey(id))
			{
				mNetEventDic[id] -= func;
			}
		}
	}
}
