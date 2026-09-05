/************************************Copyright*****************************************
 *  Project    : KNet
 *  Web        : https://github.com/1426186059/KNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/09/06 00:00:00
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
namespace KNet.Common
{
    public class NetClientMain : NetClientMainBase
    {
        public NetClientMain(NetType nNetType, ConfigInstance mConfigInstance = null)
        {
            switch (nNetType)
            {
                case NetType.WebSocketJS_V1:
                    mInterface = new KNet.WebSocket.Client.NetClientMainJS(mConfigInstance);
                    break;

                case NetType.WebSocketJS_V2:
                case NetType.WebSocket:
                default:
                    // V2 = C# 厚封装（NetClientMain + 浏览器 WebSocket 后端）。
                    mInterface = new KNet.WebSocket.Client.NetClientMain(mConfigInstance);
                    break;
            }
        }
    }
}
