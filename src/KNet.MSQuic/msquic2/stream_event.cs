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

[assembly: InternalsVisibleTo("KNet.Quic")]
namespace MSQuic2
{
    internal struct QUIC_STREAM_EVENT
    {
        public QUIC_STREAM_EVENT_TYPE Type;
        public START_COMPLETE_DATA START_COMPLETE;
        public RECEIVE_DATA RECEIVE;
        public SEND_COMPLETE_DATA SEND_COMPLETE;
        public PEER_SEND_ABORTED_DATA PEER_SEND_ABORTED;
        public PEER_RECEIVE_ABORTED_DATA PEER_RECEIVE_ABORTED;
        public SEND_SHUTDOWN_COMPLETE_DATA SEND_SHUTDOWN_COMPLETE;
        public IDEAL_SEND_BUFFER_SIZE_DATA IDEAL_SEND_BUFFER_SIZE;
        public CANCEL_ON_LOSS_DATA CANCEL_ON_LOSS;
        public SHUTDOWN_COMPLETE_DATA SHUTDOWN_COMPLETE;
        public RECEIVE_BUFFER_NEEDED_DATA RECEIVE_BUFFER_NEEDED;

        public struct START_COMPLETE_DATA
        {
            public int Status;
            public ulong ID;
            public bool PeerAccepted;
            public bool RESERVED;
        }
        
        public struct RECEIVE_DATA
        {
            public long AbsoluteOffset;
            public long TotalBufferLength;
            public QUIC_BUFFER[] Buffers;
            public int BufferCount;
            public QUIC_RECEIVE_FLAGS Flags;
        }
        
        public struct SEND_COMPLETE_DATA
        {
            public bool Canceled;
            public object ClientContext;
        }
        
        public struct PEER_SEND_ABORTED_DATA
        {
            public int ErrorCode;
        }
        
        public struct PEER_RECEIVE_ABORTED_DATA
        {
            public int ErrorCode;
        }
        
        public struct SEND_SHUTDOWN_COMPLETE_DATA
        {
            public bool Graceful;
        }
        
        public struct SHUTDOWN_COMPLETE_DATA
        {
            public bool ConnectionShutdown;
            public bool AppCloseInProgress;
            public bool ConnectionShutdownByApp;
            public bool ConnectionClosedRemotely;
            public bool RESERVED;
            public int ConnectionErrorCode;
            public int ConnectionCloseStatus;
        }
        
        public struct IDEAL_SEND_BUFFER_SIZE_DATA
        {
            public long ByteCount;
        }
        
        public struct CANCEL_ON_LOSS_DATA
        {
            public int ErrorCode;
        }

        public struct RECEIVE_BUFFER_NEEDED_DATA
        {
            public long BufferLengthNeeded;
        }
    }
}
