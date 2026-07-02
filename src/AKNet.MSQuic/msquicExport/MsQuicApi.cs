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

#if USE_MSQUIC_2 
using MSQuic2;
#else
using MSQuic1;
#endif

namespace AKNet.MSQuic.Common
{
    internal sealed class MsQuicApi
    {
        private static readonly Lazy<MsQuicApi> _lazyInstance = new Lazy<MsQuicApi>(() => new MsQuicApi());
        public QUIC_REGISTRATION Registration;
        private static readonly Version s_minMsQuicVersion = new Version(2, 0, 0);
        private static bool bInit = false;
        public static MsQuicApi Api => _lazyInstance.Value;

        public MsQuicApi()
        {
            CheckAndInit();
        }
        
        private bool CheckAndInit()
        {
            if (bInit)
            {
                return false;
            }
            bInit = true;
            if (MSQuicFunc.QUIC_FAILED(MSQuicFunc.MsQuicOpenVersion((uint)s_minMsQuicVersion.Major)))
            {
                NetLog.LogError("MSQuicFunc.MsQuicOpenVersion Error");
                return false;
            }

            var cfg = new QUIC_REGISTRATION_CONFIG
            {
                AppName = "AKNet.Quic",
                ExecutionProfile = QUIC_EXECUTION_PROFILE.QUIC_EXECUTION_PROFILE_LOW_LATENCY
            };

            if (MSQuicFunc.QUIC_FAILED(MSQuicFunc.MsQuicRegistrationOpen(cfg, out Registration)))
            {
                NetLog.LogError("MsQuicRegistrationOpen Fail");
                return false;
            }
            return true;
        }
    }
}
