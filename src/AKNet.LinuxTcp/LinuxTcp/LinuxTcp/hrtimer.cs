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

namespace AKNet.LinuxTcp.Common
{
    internal enum hrtimer_restart
    {
        HRTIMER_NORESTART,  /* Timer is not restarted */
        HRTIMER_RESTART,    /* Timer must be restarted */
    }
    
    internal class HRTimer
    {
        private readonly TimeOutGenerator _timer = new TimeOutGenerator();
        private bool bRun = false;

        private tcp_sock tcp_sock_Instance;
        private Func<tcp_sock, hrtimer_restart> _callback;

        public const byte HRTIMER_STATE_INACTIVE = 0x00;
        public const byte HRTIMER_STATE_ENQUEUED = 0x01;
        public byte state;



        //period:纳秒
        //统统都是毫秒吧
        public HRTimer(long period_ns, Func<tcp_sock, hrtimer_restart> callback, tcp_sock tcp_sock_Instance)
        {
            _timer.SetExpiresTime(NS_TO_MS(period_ns));
            this.tcp_sock_Instance = tcp_sock_Instance;
            this._callback = callback;
            this.state = HRTIMER_STATE_INACTIVE;
            bRun = false;
        }

        private long NS_TO_MS(long period_ns)
        {
            return period_ns;
        }

        public void Update(double elapsed)
        {
            if (bRun && _timer.orTimeOut())
            {
                _callback(tcp_sock_Instance);
            }
        }

        public void Start(long period_ns)
        {
            bRun = true;
            this.state = HRTIMER_STATE_ENQUEUED;
            _timer.SetExpiresTime(NS_TO_MS(period_ns));
        }

        public void Stop()
        {
            bRun = false;
            this.state = HRTIMER_STATE_INACTIVE;
        }

        public bool hrtimer_is_queued()
        {
            return LinuxTcpFunc.BoolOk(state & HRTIMER_STATE_ENQUEUED);
        }

        public void Reset()
        {
            Stop();
        }
    }
}
