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
using System;

namespace AKNet.LinuxTcp.Common
{
    internal class TimerList
    {
        private readonly TimeOutGenerator _timer = new TimeOutGenerator();
        private tcp_sock tcp_sock_Instance;
        private Action<tcp_sock> _callback;
        private bool bRun = false;

        public TimerList(long period_ms, Action<tcp_sock> callback, tcp_sock tcp_sock_Instance)
        {
            this._timer.SetExpiresTime(period_ms);
            this.tcp_sock_Instance = tcp_sock_Instance;
            this._callback = callback;
            this.bRun = false;
        }

        private long MS_TO_MS(long period_ms)
        {
            return period_ms;
        }

        public void Update(double elapsed)
        {
            if (bRun && _timer.orTimeOut())
            {
                _callback(tcp_sock_Instance);
            }
        }

        private void Start()
        {
            bRun = true;
        }

        public void Stop()
        {
            bRun = false;
        }

        public void ModTimer(long period_ms)
        {
            if (period_ms > 0)
            {
                _timer.SetExpiresTime(period_ms);
                Start();
            }
            else
            {
                Stop();
            }
        }

        public void Reset()
        {
            Stop();
        }
    }
}
