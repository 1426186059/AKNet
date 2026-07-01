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

namespace AKNet.Udp5Tcp.Common
{
    internal class TcpSlidingWindow:AkCircularManyBuffer
    {
        public uint nBeginOrderId; //待确认的OrderId

        public TcpSlidingWindow():base(Config.nUdpPackageFixedBodySize)
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
