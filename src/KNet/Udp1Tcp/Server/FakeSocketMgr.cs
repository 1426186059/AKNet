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

namespace KNet.Udp1Tcp.Server
{
    internal interface FakeSocketMgrInterface
    {
        void MultiThreadingReceiveNetPackage(SocketAsyncEventArgs e);
        void RemoveFakeSocket(FakeSocket mFakeSocket);
    }

    internal class FakeSocketMgr:FakeSocketMgrInterface
    {
        private NetServerMain mNetServer = null;
        public FakeSocketMgr(NetServerMain mNetServer)
        {
            this.mNetServer = mNetServer;
        }

        public void MultiThreadingReceiveNetPackage(SocketAsyncEventArgs e)
        {
            if (Config.bSocketSendMultiPackage)
            {
                ReadOnlySpan<byte> mBuff = e.Buffer.AsSpan().Slice(e.Offset, e.BytesTransferred);
                while (true)
                {
                    var mPackage = mNetServer.GetObjectPoolManager().NetUdpFixedSizePackage_Pop();
                    mPackage.remoteEndPoint = e.RemoteEndPoint;
                    bool bSucccess = UdpPackageEncryption.Decode(mBuff, mPackage);
                    if (bSucccess)
                    {
                        int nReadBytesCount = mPackage.Length;
                        MultiThreading_HandleSinglePackage(mPackage);
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
                        mNetServer.GetObjectPoolManager().NetUdpFixedSizePackage_Recycle(mPackage);
                        NetLog.LogError($"解码失败: {e.Buffer.Length} {e.BytesTransferred} | {mBuff.Length}");
                        break;
                    }
                }
            }
            else
            {
                NetUdpFixedSizePackage mPackage = mNetServer.GetObjectPoolManager().NetUdpFixedSizePackage_Pop();
                Buffer.BlockCopy(e.Buffer, e.Offset, mPackage.buffer, 0, e.BytesTransferred);
                mPackage.Length = e.BytesTransferred;
                mPackage.remoteEndPoint = e.RemoteEndPoint;
                bool bSucccess = UdpPackageEncryption.Decode(mPackage);
                if (bSucccess)
                {
                    MultiThreading_HandleSinglePackage(mPackage);
                }
                else
                {
                    mNetServer.GetObjectPoolManager().NetUdpFixedSizePackage_Recycle(mPackage);
                    NetLog.LogError("解码失败 !!!");
                }
            }
        }

        private void MultiThreading_HandleSinglePackage(NetUdpFixedSizePackage mPackage)
        {
            mNetServer.GetClientPeerMgr1().MultiThreading_AddPackage(mPackage);
        }
        
        public void RemoveFakeSocket(FakeSocket mFakeSocket)
        {
            
        }
    }
}