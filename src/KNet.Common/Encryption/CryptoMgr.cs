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
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("KNet.WebSocket.Server")]
[assembly: InternalsVisibleTo("KNet.WebSocket.Client")]
namespace KNet.Common
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
            if (nPackageId == 0)
            {
                return mBufferSegment;
            }
            else
            {
                return mNetPackageEncryption.Encode(nPackageId, mBufferSegment);
            }
        }

        public bool Decode(NetStreamCircularBuffer mReceiveStreamList, NetStreamReceivePackage mPackage)
        {
            if(mPackage.nPackageId == 0)
            {
                return true;
            }
            return mNetPackageEncryption.Decode(mReceiveStreamList, mPackage);
        }
    }
}
