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
        /// 发包走默认 [JSImport] byte[] 拷贝（byte[] → Uint8Array 的 marshalling 拷贝）。
        /// </summary>
        WebSocketJS_V2,
        /// <summary>
        /// 浏览器 V3：C# 厚封装 + 零拷贝视图（Zero-copy View）。
        /// 与 V2 逻辑完全一致（C# 做 XOR 帧编解码），但发包时把编码后的帧通过
        /// [JSMarshalAs&lt;JSType.MemoryView&gt;] Memory&lt;byte&gt;（指针 + 长度）直接交给 JS，
        /// 避免 byte[] → Uint8Array 的 marshalling 拷贝，用于对比“指针零拷贝”相对 V2 拷贝的发送开销。
        /// </summary>
        WebSocketJS_V3,
        /// <summary>
        /// 浏览器 V4：JS 厚封装 + 零拷贝视图（Zero-copy View）。
        /// 与 V1 逻辑一致（JS 完成 XOR 帧编解码），但发包时把原始 payload 以 MemoryView 指针方式传给 JS，
        /// 避免 C# byte[] → Uint8Array 的拷贝，JS 侧读取后编码成帧并发送，用于对比“指针零拷贝”相对 V1 拷贝的发送开销。
        /// </summary>
        WebSocketJS_V4,
    }
}
