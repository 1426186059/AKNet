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
namespace KNet.Platform
{
    public static unsafe partial class Interop
    {
        public static unsafe partial class Winsock
        {
            [Flags]
            internal enum SocketConstructorFlags
            {
                WSA_FLAG_OVERLAPPED = 0x01,
                WSA_FLAG_MULTIPOINT_C_ROOT = 0x02,
                WSA_FLAG_MULTIPOINT_C_LEAF = 0x04,
                WSA_FLAG_MULTIPOINT_D_ROOT = 0x08,
                WSA_FLAG_MULTIPOINT_D_LEAF = 0x10,
                WSA_FLAG_NO_HANDLE_INHERIT = 0x80,
            }
        }
    }
}
