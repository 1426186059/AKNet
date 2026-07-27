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
    /// <summary>
    /// 把数据拿出来
    /// </summary>
    internal static class NetPackageEncryption
	{
		private static byte[] mCheck = new byte[4] { (byte)'A', (byte)'B', (byte)'C', (byte)'D' };
	   
		public static bool DeEncryption (NetUdpFixedSizePackage mPackage)
		{
			if (mPackage.Length < Config.nUdpPackageFixedHeadSize) {
				NetLog.LogError ("mPackage Length： " + mPackage.Length);
				return false;
			}

			for (int i = 0; i < 4; i++) {
				if (mPackage.buffer [i] != mCheck [i]) {
					NetLog.LogError ("22222222222222222222222222");
					return false;
				}
			}
				
			mPackage.nPackageId = BitConverter.ToUInt16 (mPackage.buffer, 4);
			return true;
		}

		public static void Encryption (NetUdpFixedSizePackage mPackage)
		{
			UInt16 nPackageId = mPackage.nPackageId;
			Array.Copy (mCheck, 0, mPackage.buffer, 0, 4);
			byte[] byCom = BitConverter.GetBytes (nPackageId);
			Array.Copy (byCom, 0, mPackage.buffer, 4, byCom.Length);
		}
	}
}
