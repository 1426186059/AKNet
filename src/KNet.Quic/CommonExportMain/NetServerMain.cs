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
namespace KNet.Common
{
    public class NetServerMain : QuicServerMainBase
    {
        public NetServerMain(NetType nNetType)
        {
            if (nNetType == NetType.Quic)
            {
                mInterface = new KNet.Quic.Server.NetServerMain();
            }
            else if (nNetType == NetType.MSQuic)
            {
                mInterface = new KNet.MSQuic.Server.NetServerMain();
            }
            else
            {
                NetLog.LogError("Unsupported network type: " + nNetType);
            }
        }
    }
}
