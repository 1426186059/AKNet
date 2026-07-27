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
namespace KNet.LinuxTcp.Common
{
    internal static partial class LinuxTcpFunc
    {
        static bool b_inet_init = false;
        static void inet_init(tcp_sock tp)
        {
            if (!b_inet_init)
            {
                b_inet_init = true;
                tcp_init();
            }
            inet_create(tp);
        }

        static void inet_create(tcp_sock tp)
        {
            sock_init_data(tp);
        }

    }
}
