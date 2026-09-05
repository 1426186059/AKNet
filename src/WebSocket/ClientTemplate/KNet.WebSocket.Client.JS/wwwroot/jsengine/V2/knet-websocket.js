// wwwroot/jsengine/knet-websocket.js
// V2 用：JS 只封装浏览器原生 WebSocket 原语，C# 保留 KNet 全套帧编解码/心跳/重连逻辑。
// 轮询模型：JS 把二进制帧入队，C# 每帧 knet.wsReceive 取出。
// 对应 Runtime/NetClientMain_Browser.cs 的 [JSImport("knet.ws*", "main.js")]。

const instances = {};
let nextId = 1;

export const wsConnect = (url) => {
    let id = -1;
    try {
        // 注意：KNet 测试服务器是明文 ws。不要做 ws->wss 自动升级，
        // 否则会被当成 TLS 连接而秒关（ERR_CONNECTION_CLOSED）。
        // 因此性能测试页必须用 http:// 打开（不要用 https://）。
        const ws = new WebSocket(url);
        ws.binaryType = 'arraybuffer';
        id = nextId++;
        const inst = { ws, messages: [], open: false };
        ws.onopen = () => { inst.open = true; };
        ws.onmessage = (e) => {
            const data = (e.data instanceof ArrayBuffer) ? new Uint8Array(e.data) : new Uint8Array(0);
            inst.messages.push(data);
        };
        ws.onclose = () => { inst.open = false; };
        ws.onerror = (e) => { inst.open = false; console.error('[knet.ws] connection error (服务器未监听/端口不对/协议不匹配?)', e); };
        instances[id] = inst;
    } catch (e) {
        console.error('[knet.ws] connect failed', e);
        id = -1;
    }
    return id;
};

export const wsClose = (id) => {
    const inst = instances[id];
    if (inst && inst.ws) {
        try { inst.ws.close(1000, 'Normal Closure'); } catch (e) { /* ignore */ }
    }
    delete instances[id];
};

export const wsSend = (id, data) => {
    const inst = instances[id];
    if (inst && inst.ws && inst.ws.readyState === WebSocket.OPEN) {
        try {
            inst.ws.send(data);
            return 1;
        } catch (e) {
            return 0;
        }
    }
    return 0;
};

export const wsGetState = (id) => {
    const inst = instances[id];
    if (!inst || !inst.ws) return 3; // CLOSED
    return inst.ws.readyState;       // 0 CONNECTING, 1 OPEN, 2 CLOSING, 3 CLOSED
};

export const wsReceive = (id) => {
    const inst = instances[id];
    if (!inst || inst.messages.length === 0) return new Uint8Array(0);
    return inst.messages.shift();
};
