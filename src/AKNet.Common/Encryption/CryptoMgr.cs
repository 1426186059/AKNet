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
using System;

namespace AKNet.Common
{
    internal class CryptoMgr
    {
        readonly NetStreamEncryption_Xor mNetPackageEncryption = null;
        public CryptoMgr()
        {
            mNetPackageEncryption = new NetStreamEncryption_Xor();
        }

        public ReadOnlySpan<byte> Encode(ushort nPackageId, ReadOnlySpan<byte> mBufferSegment)
        {
            return mNetPackageEncryption.Encode(nPackageId, mBufferSegment);
        }

        public bool Decode(NetStreamCircularBuffer mReceiveStreamList, NetStreamReceivePackage mPackage)
        {
            return mNetPackageEncryption.Decode(mReceiveStreamList, mPackage);
        }
    }
}
