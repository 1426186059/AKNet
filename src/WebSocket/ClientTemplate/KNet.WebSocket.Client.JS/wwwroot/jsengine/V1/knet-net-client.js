// wwwroot/jsengine/knet-net-client.js
// V1 用：JS 厚封装完整 KNet 网络客户端。
//   - 浏览器原生 WebSocket 收发
//   - 复刻 KNet.Common NetStreamEncryption_Xor 的 XOR 帧编解码（头部 9 字节加密，body 明文）
//   - 消息队列（已解帧的 {packageId, body}）由 C# 每帧 knet.netReceive 取出
//   - 心跳自动发送 COMMAND_HEARTBEAT(1)
// 对应 Runtime/NetClientMainJS.cs 的 [JSImport("knet.net*", "main.js")]。
// 加密 key 由 C# [JSExport] KNetJsBridge.GetXorKeyBytes() 在初始化时注入。

const instances = {};
let nextId = 1;
let xorKey = new Uint8Array(0);

// COMMAND_HEARTBEAT = 1 (对齐 KNet.Common.CommonTcpLayerNetCommand)
const COMMAND_HEARTBEAT = 1;
const HEADER_SIZE = 9;
const CHECK = [0x4b, 0x4e, 0x45, 0x54]; // 'K','N','E','T'

export const setXorKey = (keyBytes) => {
    xorKey = new Uint8Array(keyBytes);
};

// XOR 编解码（对称）。对齐 C# XORCrypto.Encode(i, input, token)。
const xorByte = (i, input, token) => {
    const keyLen = xorKey.length;
    const idx = (i % 2 === 0)
        ? (Math.abs(i) % keyLen)
        : (Math.abs(keyLen - i) % keyLen);
    return (input ^ xorKey[idx] ^ token) & 0xff;
};

// 编码一帧：返回完整帧字节（含 9 字节头 + body）
const encodeFrame = (packageId, body) => {
    const bodyLen = body ? body.length : 0;
    const frame = new Uint8Array(HEADER_SIZE + bodyLen);
    const token = Math.floor(Math.random() * 256) & 0xff;
    frame[0] = token;
    // 明文头
    frame[1] = CHECK[0];
    frame[2] = CHECK[1];
    frame[3] = CHECK[2];
    frame[4] = CHECK[3];
    frame[5] = (packageId >> 8) & 0xff;   // BE16
    frame[6] = packageId & 0xff;
    frame[7] = (bodyLen >> 8) & 0xff;     // BE16
    frame[8] = bodyLen & 0xff;
    // XOR 加密头 [1..7]（最后 1 字节为 body 长度低位，不加密，与 C# 对齐）
    for (let i = 1; i < HEADER_SIZE - 1; i++) {
        frame[i] = xorByte(i, frame[i], token);
    }
    // body 明文
    if (bodyLen > 0) {
        frame.set(body, HEADER_SIZE);
    }
    return frame;
};

// 从累积缓冲解出所有完整帧，返回 {packageId, body} 数组
const decodeFrames = (buf) => {
    const out = [];
    let pos = 0;
    while (pos + HEADER_SIZE <= buf.length) {
        const token = buf[pos];
        // 解密头 [1..7]（与 C# 编解码范围一致）
        const head = new Uint8Array(HEADER_SIZE);
        head[0] = token;
        for (let i = 1; i < HEADER_SIZE - 1; i++) {
            head[i] = xorByte(i, buf[pos + i], token);
        }
        head[7] = buf[pos + 7];
        head[8] = buf[pos + 8];
        // 校验 'KNET'
        if (head[1] !== CHECK[0] || head[2] !== CHECK[1] ||
            head[3] !== CHECK[2] || head[4] !== CHECK[3]) {
            // 帧头损坏，丢弃一个字节继续找（容错）
            pos++;
            continue;
        }
        const packageId = (head[5] << 8) | head[6];
        const bodyLen = (head[7] << 8) | head[8];
        const total = HEADER_SIZE + bodyLen;
        if (pos + total > buf.length) break; // 帧不完整，等待更多数据
        const body = buf.slice(pos + HEADER_SIZE, pos + total);
        out.push({ packageId, body });
        pos += total;
    }
    return { frames: out, consumed: pos };
};

export const netConnect = (url) => {
    let id = -1;
    try {
        // 注意：KNet 测试服务器是明文 ws，不做 ws->wss 自动升级（否则会被当成 TLS 连接而失败）。
        // 性能测试页请用 http:// 打开。
        const ws = new WebSocket(url);
        ws.binaryType = 'arraybuffer';
        id = nextId++;
        const inst = {
            ws,
            recvBuf: new Uint8Array(0),   // 累积的原始字节
            packets: [],                   // 已解帧的 {packageId, body}
            open: false,
            lastRecv: Date.now(),
            heartbeatTimer: null,
        };
        ws.onopen = () => {
            inst.open = true;
            inst.lastRecv = Date.now();
            // 心跳定时发送
            inst.heartbeatTimer = setInterval(() => {
                if (inst.ws && inst.ws.readyState === WebSocket.OPEN) {
                    try { inst.ws.send(encodeFrame(COMMAND_HEARTBEAT, null)); } catch (e) {}
                }
            }, 2000); // fSendHeartBeatMaxTime = 2.0s
        };
        ws.onmessage = (e) => {
            const data = (e.data instanceof ArrayBuffer) ? new Uint8Array(e.data) : new Uint8Array(0);
            // 追加到接收缓冲
            const merged = new Uint8Array(inst.recvBuf.length + data.length);
            merged.set(inst.recvBuf, 0);
            merged.set(data, inst.recvBuf.length);
            inst.recvBuf = merged;
            inst.lastRecv = Date.now();
            // 解帧
            const { frames, consumed } = decodeFrames(inst.recvBuf);
            for (const f of frames) inst.packets.push(f);
            if (consumed > 0) {
                inst.recvBuf = inst.recvBuf.slice(consumed);
            }
        };
        ws.onclose = () => {
            inst.open = false;
            if (inst.heartbeatTimer) { clearInterval(inst.heartbeatTimer); inst.heartbeatTimer = null; }
        };
        ws.onerror = () => {
            inst.open = false;
            if (inst.heartbeatTimer) { clearInterval(inst.heartbeatTimer); inst.heartbeatTimer = null; }
        };
        instances[id] = inst;
    } catch (e) {
        console.error('[knet.net] connect failed', e);
        id = -1;
    }
    return id;
};

export const netClose = (id) => {
    const inst = instances[id];
    if (inst) {
        if (inst.heartbeatTimer) clearInterval(inst.heartbeatTimer);
        if (inst.ws) { try { inst.ws.close(1000, 'Normal Closure'); } catch (e) {} }
    }
    delete instances[id];
};

export const netSend = (id, packageId, data) => {
    const inst = instances[id];
    if (inst && inst.ws && inst.ws.readyState === WebSocket.OPEN) {
        try {
            inst.ws.send(encodeFrame(packageId, data));
            return 1;
        } catch (e) {
            return 0;
        }
    }
    return 0;
};

export const netGetState = (id) => {
    const inst = instances[id];
    if (!inst || !inst.ws) return 3;
    return inst.ws.readyState;
};

// 取出下一个已解帧的包。返回 Uint8Array：前 2 字节 packageId(BE16)，其余为 body。
// 无包返回空数组。这样 C# 只需一次 JSImport 即可拿到 (packageId, body)。
export const netReceive = (id) => {
    const inst = instances[id];
    if (!inst || inst.packets.length === 0) return new Uint8Array(0);
    const pkt = inst.packets.shift();
    const body = pkt.body || new Uint8Array(0);
    const out = new Uint8Array(2 + body.length);
    out[0] = (pkt.packageId >> 8) & 0xff;
    out[1] = pkt.packageId & 0xff;
    out.set(body, 2);
    return out;
};
