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
using System.Collections.Concurrent;
using System.Collections.Generic;
using KNet.Common;
using KNet.Udp.Broadcast.Common;

namespace KNet.Udp.Broadcast.Receiver
{
    public class SocketReceivePeer
	{
		internal SafeObjectPool<NetUdpFixedSizePackage> mUdpFixedSizePackagePool = null;
		protected ConcurrentQueue<NetUdpFixedSizePackage> mNeedHandlePackageQueue = null;
		protected Dictionary<UInt16, Action<ClientPeer, NetUdpFixedSizePackage>> mLogicFuncDic = null;
		protected ClientPeer clientPeer;

		public SocketReceivePeer()
        {
			clientPeer = this as ClientPeer;
			mUdpFixedSizePackagePool = new SafeObjectPool<NetUdpFixedSizePackage> (5);
			mNeedHandlePackageQueue = new ConcurrentQueue<NetUdpFixedSizePackage>();
			mLogicFuncDic = new Dictionary<ushort, Action<ClientPeer, NetUdpFixedSizePackage>> ();
        }

		public void addNetListenFun(UInt16 command, Action<ClientPeer, NetUdpFixedSizePackage> func)
		{
			if (!mLogicFuncDic.ContainsKey (command)) {
				mLogicFuncDic [command] = func;
			} else {
				mLogicFuncDic [command] += func;
			}
		}

		public void removeNetListenFun(UInt16 command, Action<ClientPeer,NetUdpFixedSizePackage> func)
		{
			if (mLogicFuncDic.ContainsKey (command)) {
				mLogicFuncDic [command] -= func;
			}
		}

		public virtual void Update(double elapsed)
		{
			int nPackageCount = 0;

			NetUdpFixedSizePackage mNetPackage = null;
			while (mNeedHandlePackageQueue.TryDequeue(out mNetPackage))
			{
				if (mLogicFuncDic.ContainsKey(mNetPackage.nPackageId))
				{
					mLogicFuncDic[mNetPackage.nPackageId](clientPeer, mNetPackage);
				}

				mUdpFixedSizePackagePool.recycle(mNetPackage);
				nPackageCount++;

				if (nPackageCount > 50)
				{
					break;
				}
			}

			if (nPackageCount > 50)
			{
				NetLog.LogWarning("广播接收器 处理逻辑的数量： " + nPackageCount);
			}
		}

        public void ReceiveUdpSocketFixedPackage(NetUdpFixedSizePackage mPackage)
		{
			bool bSucccess = NetPackageEncryption.DeEncryption (mPackage);
			if (bSucccess) {
				mNeedHandlePackageQueue.Enqueue (mPackage);
			} else {
				NetLog.LogError ("解码失败 !!!");
			}
		}


        public virtual void Dispose()
        {
			
        }

    }
}