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
    public class NetClientMain : NetClientMainBase
    {
        public NetClientMain(NetType nNetType, ConfigInstance mConfigInstance = null)
        {
            if (nNetType == NetType.Tcp)
            {
                mInterface = new KNet.Tcp.Client.NetClientMain(mConfigInstance);
            }
            else if (nNetType == NetType.Tcp2)
            {
                mInterface = new KNet.Tcp2.Client.NetClientMain(mConfigInstance);
            }
            else if (nNetType == NetType.Tcp3)
            {
                mInterface = new KNet.Tcp3.Client.NetClientMain(mConfigInstance);
            }
            else if (nNetType == NetType.Tcp4)
            {
                mInterface = new KNet.Tcp4.Client.NetClientMain(mConfigInstance);
            }
            else if (nNetType == NetType.Udp1Tcp)
            {
                mInterface = new KNet.Udp1Tcp.Client.NetClientMain(mConfigInstance);
            }
            else if (nNetType == NetType.Udp2Tcp)
            {
                mInterface = new KNet.Udp2Tcp.Client.NetClientMain(mConfigInstance);
            }
            else if (nNetType == NetType.Udp3Tcp)
            {
                mInterface = new KNet.Udp3Tcp.Client.NetClientMain(mConfigInstance);
            }
            else if (nNetType == NetType.Udp4Tcp)
            {
                mInterface = new KNet.Udp4Tcp.Client.NetClientMain(mConfigInstance);
            }
            else if (nNetType == NetType.Udp5Tcp)
            {
                mInterface = new KNet.Udp5Tcp.Client.NetClientMain(mConfigInstance);
            }
            else
            {
                NetLog.LogError("Unsupported network type: " + nNetType);
            }
        }
    }
}
