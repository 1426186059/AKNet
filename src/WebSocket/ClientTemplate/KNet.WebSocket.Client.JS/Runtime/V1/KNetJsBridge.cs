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
using System;
using System.Runtime.InteropServices.JavaScript;
using KNet.Common;

namespace KNet.WebSocket.Client
{
    /// <summary>
    /// V1（JS 厚封装）用：把 C# 侧的 XOR 加密 key 导出给 JS，
    /// 使 knet-net-client.js 的帧编解码与 KNet.Common CryptoMgr 完全一致。
    /// key 派生规则对齐 KNet.Common XORCrypto 静态构造函数：
    ///   - BuildTime.Day 为奇数：key = ASCII 字节(BuildTime.ToString("yyyy/MM/dd HH:mm:ss"))
    ///   - BuildTime.Day 为偶数：key = 8 字节 (BuildTime - DateTime.MinValue).TotalMilliseconds 的大端 Int64
    /// </summary>
    internal static partial class KNetJsBridge
    {
        [JSExport]
        public static byte[] GetXorKeyBytes()
        {
            DateTime buildTime = VersionPublishConfig.m_BuildTime;
            byte[] key;

            if (buildTime.Day % 2 == 1)
            {
                string t = buildTime.ToString("yyyy/MM/dd HH:mm:ss");
                key = new byte[t.Length];
                EndianBitConverter.SetBytes(key, 0, t);
            }
            else
            {
                key = new byte[8];
                var timeSpan = buildTime - DateTime.MinValue;
                EndianBitConverter.SetBytes(key, 0, (long)timeSpan.TotalMilliseconds);
            }

            return key;
        }

        [JSExport]
        public static double GetBuildTimeMs()
        {
            return (VersionPublishConfig.m_BuildTime - DateTime.MinValue).TotalMilliseconds;
        }
    }
}
