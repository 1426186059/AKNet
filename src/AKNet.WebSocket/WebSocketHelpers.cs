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
using System;
using System.Security.Cryptography;
using System.Text;

namespace AKNet.WebSocket
{
    internal static class WebSocketHelpers
    {
        private const string WebSocketMagicString = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public static string ComputeAcceptKey(string secWebSocketKey)
        {
            string combined = secWebSocketKey + WebSocketMagicString;
            using (var sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return Convert.ToBase64String(hash);
            }
        }
    }
}
