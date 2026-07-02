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
namespace AKNet.Common
{
    public class NetClientMain : NetClientMainBase
    {
        public NetClientMain(NetType nNetType, ConfigInstance mConfigInstance = null)
        {
            if (nNetType == NetType.Tcp)
            {
                mInterface = new AKNet.Tcp.Client.NetClientMain(mConfigInstance);
            }
            else if (nNetType == NetType.Tcp2)
            {
                mInterface = new AKNet.Tcp2.Client.NetClientMain(mConfigInstance);
            }
            else if (nNetType == NetType.Tcp3)
            {
                mInterface = new AKNet.Tcp3.Client.NetClientMain(mConfigInstance);
            }
            else if (nNetType == NetType.Tcp4)
            {
                mInterface = new AKNet.Tcp4.Client.NetClientMain(mConfigInstance);
            }
            else if (nNetType == NetType.Udp1Tcp)
            {
                mInterface = new AKNet.Udp1Tcp.Client.NetClientMain(mConfigInstance);
            }
            else if (nNetType == NetType.Udp2Tcp)
            {
                mInterface = new AKNet.Udp2Tcp.Client.NetClientMain(mConfigInstance);
            }
            else if (nNetType == NetType.Udp3Tcp)
            {
                mInterface = new AKNet.Udp3Tcp.Client.NetClientMain(mConfigInstance);
            }
            else if (nNetType == NetType.Udp4Tcp)
            {
                mInterface = new AKNet.Udp4Tcp.Client.NetClientMain(mConfigInstance);
            }
            else if (nNetType == NetType.Udp5Tcp)
            {
                mInterface = new AKNet.Udp5Tcp.Client.NetClientMain(mConfigInstance);
            }
            else
            {
                NetLog.LogError("Unsupported network type: " + nNetType);
            }
        }
    }
}
