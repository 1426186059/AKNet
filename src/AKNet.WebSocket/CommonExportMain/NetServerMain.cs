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
namespace AKNet.Common
{
    public class NetServerMain : NetServerMainBase
    {
        static NetServerMain()
        {
            AKNet.WebSocket.JslibDeployer.TryDeploy();
        }

        public NetServerMain(NetType nNetType)
        {
            mInterface = new AKNet.WebSocket.Server.NetServerMain();
        }

        public NetServerMain(NetType nNetType, ConfigInstance mConfigInstance)
        {
            mInterface = new AKNet.WebSocket.Server.NetServerMain(mConfigInstance);
        }
    }
}
