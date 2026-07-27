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
using KNet.Udp1Tcp.Common;
using System;
using System.Net.Sockets;

namespace KNet.Udp1Tcp.Client
{
    internal partial class NetClientMain
    {
        public int GetCurrentFrameRemainPackageCount()
        {
            return mWaitCheckPackageQueue.Count;
        }

        private bool NetPackageExecute()
        {
            NetUdpFixedSizePackage mPackage = null;
            lock (mWaitCheckPackageQueue)
            {
                mWaitCheckPackageQueue.TryDequeue(out mPackage);
            }

            if (mPackage != null)
            {
                UdpStatistical.AddReceivePackageCount();
                mUdpCheckPool.ReceiveNetPackage(mPackage);
                return true;
            }

            return false;
        }

        public void MultiThreading_ReceiveWaitCheckNetPackage(SocketAsyncEventArgs e)
        {
            if (Config.bSocketSendMultiPackage)
            {
                var mBuff = new ReadOnlySpan<byte>(e.Buffer, e.Offset, e.BytesTransferred);
                while (true)
                {
                    var mPackage = GetObjectPoolManager().NetUdpFixedSizePackage_Pop();
                    bool bSucccess = UdpPackageEncryption.Decode(mBuff, mPackage);
                    if (bSucccess)
                    {
                        int nReadBytesCount = mPackage.Length;

                        lock (mWaitCheckPackageQueue)
                        {
                            mWaitCheckPackageQueue.Enqueue(mPackage);
                        }

                        if (mBuff.Length > nReadBytesCount)
                        {
                            mBuff = mBuff.Slice(nReadBytesCount);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        GetObjectPoolManager().NetUdpFixedSizePackage_Recycle(mPackage);
                        NetLog.LogError($"解码失败: {e.Buffer.Length} {e.BytesTransferred} | {mBuff.Length}");
                        break;
                    }
                }
            }
            else
            {
                NetUdpFixedSizePackage mPackage = GetObjectPoolManager().NetUdpFixedSizePackage_Pop();
                Buffer.BlockCopy(e.Buffer, e.Offset, mPackage.buffer, 0, e.BytesTransferred);
                mPackage.Length = e.BytesTransferred;
                bool bSucccess = UdpPackageEncryption.Decode(mPackage);
                if (bSucccess)
                {
                    lock (mWaitCheckPackageQueue)
                    {
                        mWaitCheckPackageQueue.Enqueue(mPackage);
                    }
                }
                else
                {
                    GetObjectPoolManager().NetUdpFixedSizePackage_Recycle(mPackage);
                    NetLog.LogError("解码失败 !!!");
                }
            }
        }
    }
}