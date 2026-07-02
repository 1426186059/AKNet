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
using AKNet.MSQuic.Common;


#if USE_MSQUIC_2
using MSQuic2;
#else
using MSQuic1;
#endif

namespace AKNet.MSQuic.Common
{
    internal class MsQuicBuffers
    {
        private readonly QUIC_BUFFER[] _buffers = new QUIC_BUFFER[1];
        public QUIC_BUFFER[] Buffers => _buffers;
        public int Count => _buffers.Length;

        private void SetBuffer(int index, ReadOnlyMemory<byte> buffer)
        {
            NetLog.Assert(index < Count);
            if (_buffers[index] == null)
            {
                _buffers[index] = new QUIC_BUFFER(1024);
            }

            _buffers[index].Offset = 0;
            _buffers[index].Length = buffer.Length;
            buffer.Span.CopyTo(_buffers[index].GetSpan());
        }
        
        public void Initialize(ReadOnlyMemory<byte> buffer)
        {
            SetBuffer(0, buffer);
        }

        public void Reset()
        {
            foreach (var buffer in Buffers)
            {
                buffer.Offset = 0;
                buffer.Length = 0;
            }
        }
    }
}
