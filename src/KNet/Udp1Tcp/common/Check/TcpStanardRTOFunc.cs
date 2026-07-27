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
using System;

namespace KNet.Udp1Tcp.Common
{
    internal class TcpStanardRTOFunc
    {
        const long DefaultRtt = 1000;
        const long DefaultRttStd = 50;
        long RttOld = 0;
        long RttNew = DefaultRtt;
        long RttAverage = -1;
        long RttStdOld = 0;
        long RttStd = DefaultRttStd;

        public void FinishRttSuccess(long nRtt)
        {
            if (nRtt <= 0) return;

            RttOld = RttNew;
            RttNew = nRtt;
            RttAverage = (long)(0.125 * RttOld + (1 - 0.125) * RttNew);
            RttStdOld = RttStd;
            RttStd = (long)(0.25 * RttStdOld + (1 - 0.25) * Math.Abs(RttAverage - RttNew));
        }

        public long GetRTOTime()
        {
            if (RttAverage >= 0)
            {
                return RttAverage + 4 * RttStd;
            }
            return DefaultRtt;
        }
    }

    internal class TcpStanardRTOTimer
    {
        long nStartTime = 0;

        private long GetNowTime()
        {
            return UdpStaticCommon.GetNowTime();
        }

        public void BeginRtt()
        {
            nStartTime = GetNowTime();
        }

        public void FinishRtt(UdpClientPeerCommonBase mClientPeer)
        {
            long nRtt = GetNowTime() - nStartTime;
            mClientPeer.GetTcpStanardRTOFunc().FinishRttSuccess(nRtt);
            UdpStatistical.AddRtt(nRtt);
        }
    }
}
