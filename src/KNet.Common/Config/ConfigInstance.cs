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
    public class ConfigInstance
    {
        public int MaxPlayerCount = 10000;
        public bool bAutoReConnect = true;

        public ConfigInstance() 
        {
            bAutoReConnect = true;
            MaxPlayerCount = 10000;
        }
    }

}
