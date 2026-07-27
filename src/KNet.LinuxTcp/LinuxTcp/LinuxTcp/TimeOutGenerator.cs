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
namespace KNet.LinuxTcp.Common
{
    //这个超时器，是以毫秒为单位的
    internal class TimeOutGenerator
    {
        long fTimeOutTime = 0;
        public void SetExpiresTime(long fTimeOutTime)
        {
            this.fTimeOutTime = fTimeOutTime;
        }

        private void Stop()
        {
            this.fTimeOutTime = 0;
        }

        public bool orTimeOut()
        {
            if (this.fTimeOutTime <= 0L) { return false; }

            if (LinuxTcpFunc.tcp_jiffies32 >= fTimeOutTime)
            {
                this.Stop();
                return true;
            }
            return false;
        }
    }
}