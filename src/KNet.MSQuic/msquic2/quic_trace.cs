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

namespace MSQuic2
{
    internal enum QUIC_TRACE_API_TYPE
    {
        QUIC_TRACE_API_SET_PARAM,
        QUIC_TRACE_API_GET_PARAM,
        QUIC_TRACE_API_REGISTRATION_OPEN,
        QUIC_TRACE_API_REGISTRATION_CLOSE,
        QUIC_TRACE_API_REGISTRATION_SHUTDOWN,
        QUIC_TRACE_API_CONFIGURATION_OPEN,
        QUIC_TRACE_API_CONFIGURATION_CLOSE,
        QUIC_TRACE_API_CONFIGURATION_LOAD_CREDENTIAL,
        QUIC_TRACE_API_LISTENER_OPEN,
        QUIC_TRACE_API_LISTENER_CLOSE,
        QUIC_TRACE_API_LISTENER_START,
        QUIC_TRACE_API_LISTENER_STOP,
        QUIC_TRACE_API_CONNECTION_OPEN,
        QUIC_TRACE_API_CONNECTION_CLOSE,
        QUIC_TRACE_API_CONNECTION_SHUTDOWN,
        QUIC_TRACE_API_CONNECTION_START,
        QUIC_TRACE_API_CONNECTION_SET_CONFIGURATION,
        QUIC_TRACE_API_CONNECTION_SEND_RESUMPTION_TICKET,
        QUIC_TRACE_API_STREAM_OPEN,
        QUIC_TRACE_API_STREAM_CLOSE,
        QUIC_TRACE_API_STREAM_START,
        QUIC_TRACE_API_STREAM_SHUTDOWN,
        QUIC_TRACE_API_STREAM_SEND,
        QUIC_TRACE_API_STREAM_RECEIVE_COMPLETE,
        QUIC_TRACE_API_STREAM_RECEIVE_SET_ENABLED,
        QUIC_TRACE_API_DATAGRAM_SEND,
        QUIC_TRACE_API_CONNECTION_COMPLETE_RESUMPTION_TICKET_VALIDATION,
        QUIC_TRACE_API_CONNECTION_COMPLETE_CERTIFICATE_VALIDATION,
        QUIC_TRACE_API_STREAM_PROVIDE_RECEIVE_BUFFERS,
        QUIC_TRACE_API_COUNT // Must be last
    }

    internal static partial class MSQuicFunc
    {
        internal delegate void QUIC_TRACE_RUNDOWN_CALLBACK();
        public static void QuicTraceLogVerbose(string log)
        {
            
        }
    }
}
