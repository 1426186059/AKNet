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
using System;

namespace KNet.Udp3Tcp.Common
{
    internal class NetUdpSendFixedSizePackage : IPoolItemInterface
    {
        public readonly TcpStanardRTOTimer mTcpStanardRTOTimer = new TcpStanardRTOTimer();
        public readonly CheckPackageInfo_TimeOutGenerator mTimeOutGenerator_ReSend = new CheckPackageInfo_TimeOutGenerator();

        public TcpSlidingWindow mTcpSlidingWindow;

        public uint nOrderId;
        public uint nRequestOrderId;
        public ushort nBodyLength;

        public ushort nSendCount;

        public NetUdpSendFixedSizePackage()
        {
            this.Reset();
        }

        public void Reset()
        {
            this.nSendCount = 0;
            this.mTcpSlidingWindow = null;
            this.nRequestOrderId = 0;
            this.nOrderId = 0;
            this.nBodyLength = 0;
        }
        
        public TcpSlidingWindow WindowBuff { 
            get {
                return mTcpSlidingWindow; 
            } 
        }

        public int WindowOffset { 
            get {
                if (mTcpSlidingWindow != null)
                {
                    return mTcpSlidingWindow.GetWindowOffset(nOrderId);
                }
                return 0;
            } 
        }
        public int WindowLength { 
            get { 
                return nBodyLength; 
            } 
        }

        public bool orInnerCommandPackage()
        {
            return UdpNetCommand.orInnerCommand(GetInnerCommandId());
        }

        public byte GetInnerCommandId()
        {
            if (nOrderId < Config.nUdpMinOrderId)
            {
                return (byte)this.nOrderId;
            }
            return 0;
        }

        public void SetInnerCommandId(byte nPackageId)
        {
            this.nOrderId = nPackageId;
        }

        public void Dispose() { }
    }

    internal class NetUdpReceiveFixedSizePackage : IPoolItemInterface
    {
        public readonly byte[] mBuffer = new byte[Config.nUdpPackageFixedSize];
        public uint nOrderId;
        public uint nRequestOrderId;
        public ushort nBodyLength = 0;

        public void Reset()
        {
            this.nRequestOrderId = 0;
            this.nOrderId = 0;
        }

        public bool orInnerCommandPackage()
        {
            return UdpNetCommand.orInnerCommand(GetInnerCommandId());
        }

        public byte GetInnerCommandId()
        {
            if (nOrderId < Config.nUdpMinOrderId)
            {
                return (byte)this.nOrderId;
            }
            return 0;
        }

        public void SetInnerCommandId(byte nPackageId)
        {
            this.nOrderId = nPackageId;
        }

        public void CopyFrom(ReadOnlySpan<byte> stream)
        {
            stream.CopyTo(this.mBuffer);
        }

        public ReadOnlySpan<byte> GetTcpBufferSpan()
        {
            return mBuffer.AsSpan().Slice(0, nBodyLength);
        }

        public void Dispose() { }
    }

}

