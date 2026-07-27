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

#if USE_MSQUIC_2 
using MSQuic2;
#else
using MSQuic1;
#endif

namespace KNet.MSQuic.Common
{
    internal class ReceiveBuffers
    {
        private const int MaxBufferedBytes = 64 * 1024;
        private readonly object _syncRoot = new object();
        private readonly AkCircularManyBuffer _buffer = new AkCircularManyBuffer();
        private bool _final;

        public override string ToString()
        {
            return $"_buffer: {_buffer.Length}, MaxBufferedBytes: {MaxBufferedBytes}, _final: {_final}";
        }

        public bool HasCapacity()
        {
            lock (_syncRoot)
            {
                return _buffer.Length < MaxBufferedBytes;
            }
        }

        public int RemainLength()
        {
            lock (_syncRoot)
            {
                return _buffer.Length;
            }
        }

        public void SetFinal()
        {
            lock (_syncRoot)
            {
                _final = true;
            }
        }

        public int WriteFrom(ReadOnlySpan<QUIC_BUFFER> mBufferList, int totalLength, bool final)
        {
            lock (_syncRoot)
            {
                if (_buffer.Length + totalLength > MaxBufferedBytes)
                {
                    totalLength = MaxBufferedBytes - _buffer.Length;
                    final = false;
                }

                _final = final;

                int totalCopied = 0;
                foreach (var v in mBufferList)
                {
                    Span<byte> quicBuffer = v.GetSpan();
                    if (totalLength < quicBuffer.Length)
                    {
                        quicBuffer = quicBuffer.Slice(0, totalLength);
                    }
                    _buffer.WriteFrom(quicBuffer);
                    totalCopied += quicBuffer.Length;
                    totalLength -= quicBuffer.Length;
                }
                return totalCopied;
            }
        }

        public int WriteTo(Memory<byte> buffer, out bool completed, out bool empty)
        {
            lock (_syncRoot)
            {
                int nWriteLength = 0;
                if (!_buffer.IsEmpty)
                {
                    nWriteLength = _buffer.WriteTo(buffer.Span);
                }

                completed = _buffer.IsEmpty && _final;
                empty = _buffer.IsEmpty;
                return nWriteLength;
            }
        }

    }
}
