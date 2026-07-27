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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace KNet.Udp1Tcp.Server
{
    internal class FakeSocket : IPoolItemInterface
    {
        private readonly NetServerMain mServerMgr;
        private readonly AkCircularManySpanBuffer mWaitCheckStreamList = new AkCircularManySpanBuffer(Config.nUdpPackageFixedSize);
        private readonly Queue<NetUdpFixedSizePackage> mWaitCheckPackageQueue = new Queue<NetUdpFixedSizePackage>();
        private SOCKET_PEER_STATE mConnectionState;

        public FakeSocket(NetServerMain mServerMgr)
        {
            this.mServerMgr = mServerMgr;
            this.mConnectionState = SOCKET_PEER_STATE.DISCONNECTED;
        }

        public IPEndPoint RemoteEndPoint { get; set; }

        public void MultiThreadingReceiveNetPackage(NetUdpFixedSizePackage mPackage)
        {
            if (Config.bFakeSocketManageConnectState)
            {
                if (this.mConnectionState == SOCKET_PEER_STATE.DISCONNECTED)
                {
                    if (mPackage.nPackageId == UdpNetCommand.COMMAND_CONNECT)
                    {
                        mServerMgr.GetClientPeerMgr2().MultiThreadingHandleConnectedSocket(this);
                        this.mConnectionState = SOCKET_PEER_STATE.CONNECTED;
                    }
                }
                else if (this.mConnectionState == SOCKET_PEER_STATE.CONNECTED)
                {
                    if (mPackage.nPackageId == UdpNetCommand.COMMAND_DISCONNECT)
                    {
                        this.mConnectionState = SOCKET_PEER_STATE.DISCONNECTED;
                    }
                }

                if (this.mConnectionState == SOCKET_PEER_STATE.CONNECTED)
                {
                    lock (mWaitCheckPackageQueue)
                    {
                        mWaitCheckPackageQueue.Enqueue(mPackage);
                    }
                }
            }
            else
            {
                lock (mWaitCheckPackageQueue)
                {
                    mWaitCheckPackageQueue.Enqueue(mPackage);
                }
            }
        }

        public void MultiThreadingReceiveNetPackage(SocketAsyncEventArgs e)
        {
            if (Config.bUseReceiveCheckStream)
            {
                lock (mWaitCheckStreamList)
                {
                    mWaitCheckStreamList.WriteFromOneSpan(e.MemoryBuffer.Span.Slice(e.Offset, e.BytesTransferred));
                }
            }
            else
            {
                Span<byte> mBuff = e.MemoryBuffer.Span.Slice(e.Offset, e.BytesTransferred);
                while (true)
                {
                    var mPackage = mServerMgr.GetObjectPoolManager().NetUdpFixedSizePackage_Pop();
                    bool bSucccess = UdpPackageEncryption.Decode(mBuff, mPackage);
                    if (bSucccess)
                    {
                        int nReadBytesCount = mPackage.Length;

                        lock (mWaitCheckPackageQueue)
                        {
                            mWaitCheckPackageQueue.Enqueue(mPackage);
                            if (UdpNetCommand.orNeedCheck(mPackage.GetPackageId()))
                            {
                                //nCurrentCheckPackageCount++;
                            }
                        }

                        if (mBuff.Length > nReadBytesCount)
                        {
                            mBuff = mBuff.Slice(nReadBytesCount);
                        }
                        else
                        {
                            NetLog.Assert(mBuff.Length == nReadBytesCount);
                            break;
                        }
                    }
                    else
                    {
                        mServerMgr.GetObjectPoolManager().NetUdpFixedSizePackage_Recycle(mPackage);
                        NetLog.LogError("解码失败 !!!");
                        break;
                    }
                }
            }
        }

        public int GetCurrentFrameRemainPackageCount()
        {
            return mWaitCheckPackageQueue.Count + mWaitCheckStreamList.GetSpanCount();
        }

        public bool GetReceivePackage(out NetUdpFixedSizePackage mPackage)
        {
            GetReceivePackage();
            lock (mWaitCheckPackageQueue)
            {
                return mWaitCheckPackageQueue.TryDequeue(out mPackage);
            }
        }

        private readonly Memory<byte> mCacheBuffer = new byte[Config.nUdpPackageFixedSize];
        private void GetReceivePackage()
        {
            MainThreadCheck.Check();

            if (Config.bUseReceiveCheckStream)
            {
                Span<byte> mBuff = mCacheBuffer.Span;

                int nLength = 0;

                lock (mWaitCheckStreamList)
                {
                    if (mWaitCheckStreamList.Length > 0)
                    {
                        nLength = mWaitCheckStreamList.WriteTo(mBuff);
                    }
                }

                if (nLength > 0)
                {
                    mBuff = mBuff.Slice(0, nLength);
                    while (true)
                    {
                        var mPackage = mServerMgr.GetObjectPoolManager().NetUdpFixedSizePackage_Pop();
                        bool bSucccess = UdpPackageEncryption.Decode(mBuff, mPackage);
                        if (bSucccess)
                        {
                            int nReadBytesCount = mPackage.Length;
                            mWaitCheckPackageQueue.Enqueue(mPackage);
                            if (mBuff.Length > nReadBytesCount)
                            {
                                mBuff = mBuff.Slice(nReadBytesCount);
                            }
                            else
                            {
                                NetLog.Assert(mBuff.Length == nReadBytesCount);
                                break;
                            }
                        }
                        else
                        {
                            mServerMgr.GetObjectPoolManager().NetUdpFixedSizePackage_Recycle(mPackage);
                            NetLog.LogError("解码失败 !!!");
                            break;
                        }
                    }
                }
            }
        }

        public bool SendToAsync(SocketAsyncEventArgs mArg)
        {
            return this.mServerMgr.SendToAsync(mArg);
        }

        public void Reset()
        {
            lock (mWaitCheckPackageQueue)
            {
                while (mWaitCheckPackageQueue.TryDequeue(out var mPackage))
                {
                    mServerMgr.GetObjectPoolManager().NetUdpFixedSizePackage_Recycle(mPackage);
                }
            }

            lock (mWaitCheckStreamList)
            {
                mWaitCheckStreamList.Reset();
            }
        }

        public void Close()
        {
            this.mServerMgr.GetFakeSocketMgr().RemoveFakeSocket(this);
        }

        public void Dispose() { }
    }
}
