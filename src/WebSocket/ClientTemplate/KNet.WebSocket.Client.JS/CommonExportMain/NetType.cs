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

namespace KNet.Common
{
    public enum NetType
    {
        /// <summary>保留：桌面端 C# ClientWebSocket（浏览器不可用）。</summary>
        WebSocket,
        /// <summary>
        /// 浏览器 V1：JS 厚封装。
        /// JS 实现完整网络客户端（WebSocket + 帧编解码 + 消息队列 + 心跳/重连/状态），
        /// C# 层只做 JSImport 薄调用。用于对比「C# ↔ JS 互操作开销」。
        /// </summary>
        WebSocketJS_V1,
        /// <summary>
        /// 浏览器 V2：C# 厚封装。
        /// JS 只提供 WebSocket 原语（connect/send/receive/getState），
        /// C# 保留 KNet 全套逻辑（环形缓冲 / CryptoMgr XOR 编解码 / 心跳 / 重连 / 包监听）。
        /// 移植自 Web_Mir3 BrowserWebSocket + JsWebSocketTransport。
        /// </summary>
        WebSocketJS_V2,
    }
}
