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
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace MSQuic2
{
    internal static class CxPlatRandom
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Random(QUIC_SSBuffer randomBytes)
        {
            RandomNumberGenerator.Fill(randomBytes.GetSpan());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Random(Span<byte> randomBytes)
        {
            RandomNumberGenerator.Fill(randomBytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Random(byte[] randomBytes)
        {
            RandomNumberGenerator.Fill(randomBytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Random(ref int randomBytes)
        {
            randomBytes = RandomNumberGenerator.GetInt32(0, int.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Random(ref byte randomBytes)
        {
            randomBytes = (byte)RandomNumberGenerator.GetInt32(0, byte.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Random(ref uint randomBytes)
        {
            randomBytes = (uint)RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte RandomByte()
        {
            return (byte)RandomNumberGenerator.GetInt32(0, byte.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RandomInt32()
        {
            return (Int32)RandomNumberGenerator.GetInt32(0, int.MaxValue);
        }
    }
}
