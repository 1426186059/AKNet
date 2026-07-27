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
using KNet.LinuxTcp.Common;
using System;
using System.Net.Sockets;

namespace KNet.LinuxTcp.Client
{
    internal partial class NetClientMain
    {
        private bool NetCheckPackageExecute()
        {
            sk_buff mPackage = null;
            lock (mWaitCheckPackageQueue)
            {
                mWaitCheckPackageQueue.TryDequeue(out mPackage);
            }

            if (mPackage != null)
            {
                mUdpCheckPool.ReceiveNetPackage(mPackage);
                return true;
            }

            return false;
        }

        public void MultiThreading_ReceiveWaitCheckNetPackage(SocketAsyncEventArgs e)
        {
            ReadOnlySpan<byte> mBuff = e.MemoryBuffer.Span.Slice(e.Offset, e.BytesTransferred);
            var skb = GetObjectPoolManager().Skb_Pop();
            skb = LinuxTcpFunc.build_skb(skb, mBuff);

            lock (mWaitCheckPackageQueue)
            {
                mWaitCheckPackageQueue.Enqueue(skb);
            }
        }

        private bool NetTcpPackageExecute()
        {
            bool bSuccess = mCryptoMgr.Decode(mReceiveStreamList, mNetPackage);
            if (bSuccess)
            {
                NetPackageExecute(mNetPackage);
            }
            return bSuccess;
        }

        public void ReceiveTcpStream()
        {
            while (mUdpCheckPool.ReceiveTcpStream(mTcpMsg))
            {
                while (NetTcpPackageExecute())
                {

                }
            }
        }
    }
}
