/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库
*        Author:许珂
*        StartTime:2024/11/01 00:00:00
*        ModifyTime:2026/2/1 20:26:47
*        Copyright:MIT软件许可证
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
