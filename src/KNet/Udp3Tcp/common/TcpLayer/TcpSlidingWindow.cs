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

namespace KNet.Udp3Tcp.Common
{
    internal class TcpSlidingWindow : AkCircularManyBuffer
    {
        public uint nBeginOrderId; //待确认的OrderId

        public TcpSlidingWindow() : base(Config.nUdpPackageFixedBodySize)
        {
            nBeginOrderId = Config.nUdpMinOrderId;
        }

        public void DoWindowForward(uint nRequestOrderId)
        {
            int nClearLength = GetWindowOffset(nRequestOrderId);
            ClearBuffer(nClearLength);
            nBeginOrderId = nRequestOrderId;
        }

        public int GetWindowOffset(uint nOrderId)
        {
            return OrderIdHelper.GetOrderIdLength(nBeginOrderId, nOrderId);
        }

        public void WindowReset()
        {
            base.Reset();
            nBeginOrderId = Config.nUdpMinOrderId;
        }
    }
}
