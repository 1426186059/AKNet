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
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace KNet.Udp1Tcp.Server
{
    internal class InnerCommandSendMgr
    {
        readonly SocketAsyncEventArgs SendArgs = new SocketAsyncEventArgs();
        readonly SafeObjectPool<NetUdpFixedSizePackage> mPackagePool = new SafeObjectPool<NetUdpFixedSizePackage>();
        readonly ConcurrentQueue<NetUdpFixedSizePackage> mSendPackageQueue = new ConcurrentQueue<NetUdpFixedSizePackage>();

        private readonly NetServerMain mNetServer = null;
        private bool bSendIOContexUsed = false;

        public InnerCommandSendMgr(NetServerMain mNetServer)
        {
            this.mNetServer = mNetServer;
            SendArgs.Completed += ProcessSend;
            SendArgs.SetBuffer(new byte[Config.nUdpPackageFixedSize], 0, Config.nUdpPackageFixedSize);
        }

        public void SendInnerNetData(ushort nId, EndPoint removeEndPoint)
        {
            NetLog.Assert(UdpNetCommand.orInnerCommand(nId));

            NetUdpFixedSizePackage mPackage = mPackagePool.Pop();
            mPackage.nPackageId = nId;
            mPackage.Length = Config.nUdpPackageFixedHeadSize;
            mPackage.remoteEndPoint = removeEndPoint;
            UdpPackageEncryption.Encode(mPackage);
            SendNetPackage(mPackage);
        }

        private void ProcessSend(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                SendNetPackage2(e);
            }
            else
            {
                bSendIOContexUsed = false;
            }
        }

        private void SendNetPackage(NetUdpFixedSizePackage mPackage)
        {
            MainThreadCheck.Check();
            if (Config.bUseSendAsync)
            {
                mSendPackageQueue.Enqueue(mPackage);

                if (!bSendIOContexUsed)
                {
                    bSendIOContexUsed = true;
                    SendNetPackage2(SendArgs);
                }
            }
            else
            {
                mNetServer.SendTo(mPackage);
                mPackagePool.recycle(mPackage);
            }
        }

        private void SendNetPackage2(SocketAsyncEventArgs e)
        {
            NetUdpFixedSizePackage mPackage = null;
            if (mSendPackageQueue.TryDequeue(out mPackage))
            {
                Array.Copy(mPackage.buffer, e.Buffer, mPackage.Length);
                e.SetBuffer(0, mPackage.Length);
                e.RemoteEndPoint = mPackage.remoteEndPoint;
                mPackagePool.recycle(mPackage);

                if (!mNetServer.SendToAsync(e))
                {
                    ProcessSend(null, e);
                }
            }
            else
            {
                bSendIOContexUsed = false;
            }
        }
    }
}