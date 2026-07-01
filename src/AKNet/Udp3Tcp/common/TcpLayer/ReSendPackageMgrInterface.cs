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

namespace AKNet.Udp3Tcp.Common
{
    internal class CheckPackageInfo_TimeOutGenerator
    {
        double fTime = 0;
        double fInternalTime = -1;
        public void SetInternalTime(double fInternalTime)
        {
            this.Reset();
            this.fInternalTime = fInternalTime;
        }

        public void Reset()
        {
            this.fInternalTime = -1;
            this.fTime = 0.0;
        }

        public bool orSetInternalTime()
        {
            return fInternalTime >= 0.0;
        }

        public bool orTimeOut(double fElapsed)
        {
            this.fTime += fElapsed;
            if (this.fTime >= fInternalTime)
            {
                this.Reset();
                return true;
            }

            return false;
        }
    }

    internal interface ReSendPackageMgrInterface
    {
        void AddTcpStream(ReadOnlySpan<byte> buffer);
        void ReceiveOrderIdRequestPackage(uint nRequestOrderId);
        void Update(double elapsed);
        void Reset();
    }

}