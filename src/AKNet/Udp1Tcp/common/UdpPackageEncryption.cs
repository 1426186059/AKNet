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

namespace AKNet.Udp1Tcp.Common
{
    /// <summary>
    /// 把数据拿出来
    /// </summary>
    internal static class UdpPackageEncryption
    {
        private static readonly byte[] mCheck = new byte[4] { (byte)'$', (byte)'$', (byte)'$', (byte)'$' };
        public static bool Decode(NetUdpFixedSizePackage mPackage)
        {
            if (mPackage.Length < Config.nUdpPackageFixedHeadSize)
            {
                return false;
            }

            for (int i = 0; i < 4; i++)
            {
                if (mPackage.buffer[i] != mCheck[i])
                {
                    return false;
                }
            }

            mPackage.nOrderId = EndianBitConverter.ToUInt16(mPackage.buffer, 4);
            mPackage.nGroupCount = EndianBitConverter.ToUInt16(mPackage.buffer, 6);
            mPackage.nPackageId = EndianBitConverter.ToUInt16(mPackage.buffer, 8);
            mPackage.nRequestOrderId = EndianBitConverter.ToUInt16(mPackage.buffer, 10);
            return true;
        }

        public static bool Decode(ReadOnlySpan<byte> mBuff, NetUdpFixedSizePackage mPackage)
        {
            if (mBuff.Length < Config.nUdpPackageFixedHeadSize)
            {
                NetLog.LogError($"解码失败 1: {mBuff.Length} | {Config.nUdpPackageFixedHeadSize}");
                return false;
            }

            for (int i = 0; i < 4; i++)
            {
                if (mBuff[i] != mCheck[i])
                {
                    NetLog.LogError($"解码失败 2");
                    return false;
                }
            }

            mPackage.nOrderId = EndianBitConverter.ToUInt16(mBuff.Slice(4, 2));
            mPackage.nGroupCount = EndianBitConverter.ToUInt16(mBuff.Slice(6, 2));
            mPackage.nPackageId = EndianBitConverter.ToUInt16(mBuff.Slice(8, 2));
            mPackage.nRequestOrderId = EndianBitConverter.ToUInt16(mBuff.Slice(10, 2));
            ushort nBodyLength = EndianBitConverter.ToUInt16(mBuff.Slice(12, 2));

            if (Config.nUdpPackageFixedHeadSize + nBodyLength > Config.nUdpPackageFixedSize)
            {
                NetLog.LogError($"解码失败 3: {nBodyLength} | {Config.nUdpPackageFixedSize}");
                return false;
            }

            try
            {
                mPackage.CopyFrom(mBuff.Slice(Config.nUdpPackageFixedHeadSize, nBodyLength));
            }
            catch(Exception e)
            {
                NetLog.LogError(mBuff.Length + " | " + nBodyLength);
                NetLog.LogException(e);
                return false;
            }
            return true;
        }

        public static bool InnerCommandPeek(ReadOnlySpan<byte> mBuff, InnectCommandPeekPackage mPackage)
        {
            if (mBuff.Length < Config.nUdpPackageFixedHeadSize)
            {
                return false;
            }

            for (int i = 0; i < 4; i++)
            {
                if (mBuff[i] != mCheck[i])
                {
                    return false;
                }
            }

            ushort nBodyLength = EndianBitConverter.ToUInt16(mBuff.Slice(12));
            if (nBodyLength != 0)
            {
                return false;
            }

            mPackage.nPackageId = EndianBitConverter.ToUInt16(mBuff.Slice(8));
            mPackage.Length = Config.nUdpPackageFixedHeadSize;
            return true;
        }

        public static void Encode(NetUdpFixedSizePackage mPackage)
        {
            ushort nOrderId = mPackage.nOrderId;
            ushort nGroupCount = mPackage.nGroupCount;
            ushort nPackageId = mPackage.nPackageId;
            ushort nSureOrderId = mPackage.nRequestOrderId;
            ushort nBodyLength = (ushort)(mPackage.Length - Config.nUdpPackageFixedHeadSize);

            Array.Copy(mCheck, 0, mPackage.buffer, 0, 4);
            EndianBitConverter.SetBytes(mPackage.buffer, 4, nOrderId);
            EndianBitConverter.SetBytes(mPackage.buffer, 6, nGroupCount);
            EndianBitConverter.SetBytes(mPackage.buffer, 8, nPackageId);
            EndianBitConverter.SetBytes(mPackage.buffer, 10, nSureOrderId);
            EndianBitConverter.SetBytes(mPackage.buffer, 12, nBodyLength);
        }

	}
}
