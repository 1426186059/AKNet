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
using System.Security.Cryptography;
using System.Text;

namespace KNet.WebSocket
{
    public static class WebSocketHelpers
    {
        private const string WebSocketMagicString = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        private static readonly System.Security.Cryptography.SHA1 s_SHA1 = System.Security.Cryptography.SHA1.Create();

        public static string ComputeAcceptKey(string secWebSocketKey)
        {
            string combined = secWebSocketKey + WebSocketMagicString;
            // 握手在主线程(单线程)完成，复用单个 SHA1 实例，避免每次握手都 new/Dispose 带来的分配与 GC 压力。
#pragma warning disable SYSLIB0021
            byte[] hash = s_SHA1.ComputeHash(Encoding.UTF8.GetBytes(combined));
#pragma warning restore SYSLIB0021
            return Convert.ToBase64String(hash);
        }

    }
}
