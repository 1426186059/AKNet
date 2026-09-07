/************************************Copyright*****************************************
 *  Project    : KNet
 *  Web        : https://github.com/1426186059/KNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/09/07 00:00:00
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using System;
using System.Runtime.CompilerServices;
using KNet.Common;

namespace KNet.WebSocket.Client
{
    /// <summary>
    /// 浏览器 WASM(JS) 路径专用 XOR 编解码。逻辑与 NetStreamEncryption_Xor 完全一致，
    /// 但 Encode 返回 ArraySegment&lt;byte&gt;（而非 ReadOnlySpan）、Decode 把包体以
    /// ArraySegment&lt;byte&gt; 存入 NetStreamReceivePackage——二者都更适合交给 WebSocket
    /// [JSImport] 收发：ArraySegment 可直接取出底层 byte[] 与偏移/长度，既支持零拷贝指针发送
    /// (MemoryView)，也支持 byte[] 拷贝发送，而 ReadOnlySpan 无法跨 JS 边界且拿不到 byte[]。
    /// </summary>
    internal sealed class JsXorCodec
    {
        private const int nPackageFixedHeadSize = 9;
        private static readonly byte[] mCheck = new byte[] { (byte)'K', (byte)'N', (byte)'E', (byte)'T' };
        private byte[] mCacheSendBuffer = new byte[1024];
        private byte[] mCacheReceiveBuffer = new byte[1024];
        private byte[] mCacheHead = new byte[nPackageFixedHeadSize];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnSureSendBufferOk(int nSumLength)
        {
            BufferTool.EnSureBufferOk_Power2(ref mCacheSendBuffer, nSumLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnSureReceiveBufferOk(int nSumLength)
        {
            BufferTool.EnSureBufferOk_Power2(ref mCacheReceiveBuffer, nSumLength);
        }

        public ArraySegment<byte> Encode(ushort nPackageId, ReadOnlySpan<byte> mBufferSegment)
        {
            int nSumLength = mBufferSegment.Length + nPackageFixedHeadSize;
            EnSureSendBufferOk(nSumLength);

            byte nEncodeToken = (byte)RandomTool.RandomInt32(0, byte.MaxValue);
            mCacheSendBuffer[0] = nEncodeToken;
            mCheck.CopyTo(mCacheSendBuffer, 1);
            EndianBitConverter.SetBytes(mCacheSendBuffer, 5, (ushort)nPackageId);
            EndianBitConverter.SetBytes(mCacheSendBuffer, 7, (ushort)mBufferSegment.Length);

            for (int i = 1; i < nPackageFixedHeadSize - 1; i++)
            {
                mCacheSendBuffer[i] = XORCrypto.Encode(i, mCacheSendBuffer[i], nEncodeToken);
            }

            if (mBufferSegment.Length > 0)
            {
                mBufferSegment.CopyTo(mCacheSendBuffer.AsSpan(nPackageFixedHeadSize));
            }
            return new ArraySegment<byte>(mCacheSendBuffer, 0, nSumLength);
        }

        public bool Decode(NetStreamCircularBuffer mReceiveStreamList, NetStreamReceivePackage mPackage)
        {
            if (mReceiveStreamList.Length < nPackageFixedHeadSize)
            {
                return false;
            }

            mReceiveStreamList.CopyTo(mCacheHead);
            byte nEncodeToken = mCacheHead[0];
            for (int i = 1; i < nPackageFixedHeadSize - 1; i++)
            {
                mCacheHead[i] = XORCrypto.Encode(i, mCacheHead[i], nEncodeToken);
            }

            for (int i = 0; i < mCheck.Length; i++)
            {
                if (mCheck[i] != mCacheHead[i + 1])
                {
                    return false;
                }
            }

            ushort nPackageId = EndianBitConverter.ToUInt16(mCacheHead, 5);
            int nBodyLength = EndianBitConverter.ToUInt16(mCacheHead, 7);
            NetLog.Assert(nBodyLength >= 0);

            int nSumLength = nBodyLength + nPackageFixedHeadSize;
            if (!mReceiveStreamList.isCanWriteTo(nSumLength))
            {
                return false;
            }

            mReceiveStreamList.ClearBuffer(nPackageFixedHeadSize);
            if (nBodyLength > 0)
            {
                EnSureReceiveBufferOk(nBodyLength);
                Span<byte> mCacheReceiveBufferSpan = mCacheReceiveBuffer.AsSpan();
                mReceiveStreamList.WriteTo(mCacheReceiveBufferSpan.Slice(0, nBodyLength));
            }

            mPackage.nPackageId = nPackageId;
            mPackage.SetData(new ArraySegment<byte>(mCacheReceiveBuffer, 0, nBodyLength));
            return true;
        }
    }
}
