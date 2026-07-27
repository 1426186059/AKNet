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

namespace KNet.Common.Channel
{
    public partial class ChannelClosedException : InvalidOperationException
    {
        public ChannelClosedException() :
            base() { }

        public ChannelClosedException(string? message) : base(message) { }
        public ChannelClosedException(Exception? innerException) : base() { }
        public ChannelClosedException(string? message, Exception? innerException) : base(message) { }
    }
}
