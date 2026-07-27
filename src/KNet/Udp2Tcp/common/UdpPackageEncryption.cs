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

namespace KNet.Udp2Tcp.Common
{
    internal static class UdpPackageEncryption
    {
        private static readonly byte[] mCheck = new byte[2] { (byte)'$', (byte)'$'};
        public static bool Decode(ReadOnlySpan<byte> mBuff, NetUdpFixedSizePackage mPackage)
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

            mPackage.nOrderId = EndianBitConverter.ToUInt16(mBuff.Slice(2));
            if (mPackage.nOrderId == 0)
            {
                NetLog.LogError($"解码失败 3");
                return false;
            }

            mPackage.nRequestOrderId = EndianBitConverter.ToUInt16(mBuff.Slice(4));
            ushort nBodyLength = EndianBitConverter.ToUInt16(mBuff.Slice(6));

            if (Config.nUdpPackageFixedHeadSize + nBodyLength > CommonUdpLayerConfig.nUdpPackageFixedSize)
            {
                NetLog.LogError($"解码失败 4: {nBodyLength} | {CommonUdpLayerConfig.nUdpPackageFixedSize}");
                return false;
            }

            mPackage.CopyFrom(mBuff.Slice(Config.nUdpPackageFixedHeadSize, nBodyLength));
            return true;
        }

        public static void Encode(NetUdpFixedSizePackage mPackage)
        {
            ushort nOrderId = mPackage.nOrderId;
            ushort nRequestOrderId = mPackage.nRequestOrderId;
            ushort nBodyLength = (ushort)(mPackage.Length - Config.nUdpPackageFixedHeadSize);

            Array.Copy(mCheck, 0, mPackage.buffer, 0, 2);
            EndianBitConverter.SetBytes(mPackage.buffer, 2, nOrderId);
            EndianBitConverter.SetBytes(mPackage.buffer, 4, nRequestOrderId);
            EndianBitConverter.SetBytes(mPackage.buffer, 6, nBodyLength);
        }

	}
}
