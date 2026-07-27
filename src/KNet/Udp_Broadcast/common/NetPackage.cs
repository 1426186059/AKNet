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
using KNet.Common;

namespace KNet.Udp.Broadcast.Common
{
    public class NetUdpFixedSizePackage: IPoolItemInterface
	{
		public UInt16 nPackageId;

		public byte[] buffer;
		public int Length;

		public NetUdpFixedSizePackage ()
		{
			nPackageId = 0;
			Length = 0;
			buffer = new byte[Config.nUdpPackageFixedSize];
		}

        public void Reset()
        {
			nPackageId = 0;
			Length = 0;
        }

        public void Dispose() { }
    }
}

