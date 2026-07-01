/************************************Copyright*****************************************
 *  Project    : AKNet
 *  Web        : https://github.com/825126369/AKNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/07/02 04:26:51
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using System;

namespace AKNet.Common
{
    internal static unsafe class UnSafeTool
    {
        public static void DeepCloneStruct<T>(ref T source, ref T target) where T : struct
        {
            int size = sizeof(T);
            // 固定源和目标内存地址
            fixed (T* pSrc = &source)
            fixed (T* pDest = &target)
            {
                Buffer.MemoryCopy(pSrc, pDest, size, size);
            }
        }

        public static ReadOnlySpan<byte> GetSpan<T>(T* source) where T : struct
        {
            return new ReadOnlySpan<byte>((void*)source, sizeof(T));
        }

    }
}
