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
namespace AKNet.MSQuic.Common
{
    internal static class Config
    {
        //Common
        public const int nIOContexBufferLength = 1024;
        public const int nDataMaxLength = ushort.MaxValue;
        public const double fReceiveHeartBeatTimeOut = 5.0;
        public const double fMySendHeartBeatMaxTime = 2.0;
        public const double fReConnectMaxCdTime = 3.0;
        public const int MaxPlayerCount = 10000;

        public const int DefaultCloseErrorCode = byte.MaxValue;
        public const int DefaultStreamErrorCode = ushort.MaxValue;
    }
}
