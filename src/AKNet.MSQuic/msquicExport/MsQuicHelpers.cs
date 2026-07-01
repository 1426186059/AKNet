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
using AKNet.Common;
using System.Net;

#if USE_MSQUIC_2 
using MSQuic2;
#else
using MSQuic1;
#endif

namespace AKNet.MSQuic.Common
{
    internal static class MsQuicHelpers
    {
        internal static bool TryParse(this EndPoint endPoint, out string? host, out IPAddress? address, out int port)
        {
            if (endPoint is DnsEndPoint dnsEndPoint)
            {
                host = IPAddress.TryParse(dnsEndPoint.Host, out address) ? null : dnsEndPoint.Host;
                port = dnsEndPoint.Port;
                return true;
            }

            if (endPoint is IPEndPoint ipEndPoint)
            {
                host = null;
                address = ipEndPoint.Address;
                port = ipEndPoint.Port;
                return true;
            }

            host = default;
            address = default;
            port = default;
            return false;
        }

        public static void SetMsQuicParameter(QUIC_HANDLE handle, uint parameter, QUIC_SSBuffer value)
        {
#if USE_MSQUIC_2
            int status = MSQuicFunc.MsQuicSetParam(handle, parameter, value);
            if (MSQuicFunc.QUIC_FAILED(status))
            {
                NetLog.LogError($"SetParam({handle}, {parameter}) failed");
            }
#endif
        }

    }
}
