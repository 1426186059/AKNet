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
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("KNet")]
[assembly: InternalsVisibleTo("KNet.MSQuic")]
[assembly: InternalsVisibleTo("KNet.LinuxTcp")]
[assembly: InternalsVisibleTo("KNet.WebSocket")]
[assembly: InternalsVisibleTo("KNet.Quic")]
[assembly: InternalsVisibleTo("KNet.Quic2Tcp")]
namespace KNet.Common
{
    internal class NetStreamCircularBuffer:AkCircularManyBuffer
    {
        //AkCircularBuffer mBufferInterface;

        //public int Length
        //{
        //    get
        //    {
        //        return mBufferInterface.Length;
        //    }
        //}

        //public void WriteFrom(SocketAsyncEventArgs e)
        //{
        //    mBufferInterface.WriteFrom(e.MemoryBuffer.Span.Slice(e.Offset, e.BytesTransferred));
        //}

        //public bool isCanWriteTo(int countT)
        //{
        //    return mBufferInterface.isCanWriteTo(countT);
        //}

        //public int CopyTo(Span<byte> mTempSpan)
        //{
        //    return mBufferInterface.CopyTo(0, mTempSpan);
        //}

        //public void Reset()
        //{

        //}

        //public void Dispose()
        //{

        //}
    }
}
