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
using System.Runtime.CompilerServices;

namespace KNet.Udp3Tcp.Common
{
    internal static class OrderIdHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AddOrderId(uint nOrderId)
        {
            return AddOrderId(nOrderId, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint MinusOrderId(uint nOrderId)
        {
            return AddOrderId(nOrderId, -1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AddOrderId(uint nOrderId, int nAddCount)
        {
            long n2 = nOrderId + nAddCount;
            if (n2 > Config.nUdpMaxOrderId)
            {
                n2 = n2 - Config.nUdpMaxOrderId + Config.nUdpMinOrderId - 1;
            }
            else if (n2 < Config.nUdpMinOrderId)
            {
                n2 = n2 + Config.nUdpMaxOrderId - Config.nUdpMinOrderId + 1;
            }

            NetLog.Assert(n2 >= Config.nUdpMinOrderId && n2 <= Config.nUdpMaxOrderId, n2);
            uint n3 = (uint)n2;
            return n3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool orInOrderIdFront(uint nOrderId_Back, uint nOrderId, int nCount)
        {
            if (nOrderId_Back + nCount <= Config.nUdpMaxOrderId)
            {
                return nOrderId > nOrderId_Back && nOrderId <= nOrderId_Back + nCount;
            }
            else
            {
                if (nOrderId > nOrderId_Back)
                {
                    return nOrderId > nOrderId_Back && nOrderId <= Config.nUdpMaxOrderId;
                }
                else
                {
                    return nOrderId >= Config.nUdpMinOrderId && nOrderId <= AddOrderId(nOrderId_Back, nCount);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetOrderIdLength(uint nOrderId, uint nRequestOrderId)
        {
            if (nRequestOrderId >= nOrderId)
            {
                return (int)(nRequestOrderId - nOrderId);
            }
            else
            {
                return (int)(Config.nUdpMaxOrderId - nOrderId + nRequestOrderId - Config.nUdpMinOrderId + 1);
            }
        }
    }
}
