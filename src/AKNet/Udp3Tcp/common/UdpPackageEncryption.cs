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
using AKNet.Common;
using System;

namespace AKNet.Udp3Tcp.Common
{
    /// <summary>
    /// 把数据拿出来
    /// </summary>
    internal static class UdpPackageEncryption
    {
        private static readonly byte[] mCheck = new byte[2] { (byte)'$', (byte)'$'};
        private static readonly byte[] mCacheSendHeadBuffer = new byte[Config.nUdpPackageFixedHeadSize];

        public static bool Decode(ReadOnlySpan<byte> mBuff, NetUdpReceiveFixedSizePackage mPackage)
        {
            if (mBuff.Length < Config.nUdpPackageFixedHeadSize)
            {
                NetLog.LogError($"解码失败 1: {mBuff.Length} | {Config.nUdpPackageFixedHeadSize}");
                return false;
            }

            for (int i = 0; i < 2; i++)
            {
                if (mBuff[i] != mCheck[i])
                {
                    NetLog.LogError($"解码失败 2");
                    return false;
                }
            }

            ushort nBodyLength = EndianBitConverter.ToUInt16(mBuff.Slice(10));
            if (Config.nUdpPackageFixedHeadSize + nBodyLength > Config.nUdpPackageFixedSize)
            {
                NetLog.LogError($"解码失败 3: {nBodyLength} | {Config.nUdpPackageFixedSize}");
                return false;
            }

            mPackage.nBodyLength = nBodyLength;
            mPackage.nOrderId = EndianBitConverter.ToUInt32(mBuff.Slice(2));
            mPackage.nRequestOrderId = EndianBitConverter.ToUInt32(mBuff.Slice(6));
            mPackage.CopyFrom(mBuff.Slice(Config.nUdpPackageFixedHeadSize, nBodyLength));
            return true;
        }
        
        public static ReadOnlySpan<byte> EncodeHead(NetUdpSendFixedSizePackage mPackage)
        {
            uint nOrderId = mPackage.nOrderId;
            uint nRequestOrderId = mPackage.nRequestOrderId;
            ushort nBodyLength = (ushort)mPackage.nBodyLength;

            Buffer.BlockCopy(mCheck, 0, mCacheSendHeadBuffer, 0, 2);
            EndianBitConverter.SetBytes(mCacheSendHeadBuffer, 2, nOrderId);
            EndianBitConverter.SetBytes(mCacheSendHeadBuffer, 6, nRequestOrderId);
            EndianBitConverter.SetBytes(mCacheSendHeadBuffer, 10, nBodyLength);

            return mCacheSendHeadBuffer;
        }

	}
}
