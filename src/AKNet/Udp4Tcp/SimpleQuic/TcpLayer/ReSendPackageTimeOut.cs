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
namespace AKNet.Udp4Tcp.Common
{
    internal class ReSendPackageTimeOut
    {
        long fLastTime = 0;
        long fInternalTime = -1;
        private LogicWorker mLogicWorker;
        public void SetInternalTime(long fInternalTime)
        {
            this.Reset();
            this.fInternalTime = fInternalTime;
        }

        public void Reset()
        {
            this.fInternalTime = -1;
            this.fLastTime = mLogicWorker.mThreadWorker.TimeNow;
        }

        public bool orSetInternalTime()
        {
            return fInternalTime >= 0.0;
        }

        public bool orTimeOut()
        {
            if (mLogicWorker.mThreadWorker.TimeNow - fLastTime >= fInternalTime)
            {
                this.Reset();
                return true;
            }

            return false;
        }

        public void SetLogicWorker(LogicWorker mLogicWorker)
        {
            this.mLogicWorker = mLogicWorker;
        }
    }
}