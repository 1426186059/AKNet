// wwwroot/jsengine/knet-websocket.js
// V2 用：JS 只封装浏览器原生 WebSocket 原语，C# 保留 KNet 全套帧编解码/心跳/重连逻辑。
// 轮询模型：JS 把二进制帧入队，C# 每帧 knet.wsReceive 取出。
// 对应 Runtime/NetClientMain_Browser.cs 的 [JSImport("knet.ws*", "main.js")]。

const instances = {};
let nextId = 1;

export const wsConnect = (url) => {
    let id = -1;
    try {
        // HTTPS 页面下浏览器禁止混合内容（ws://），自动升级为 wss://
        if (typeof location !== 'undefined' && location.protocol === 'https:' && url.startsWith('ws://')) {
            url = 'wss://' + url.slice('ws://'.length);
        }
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
        ws.onerror = () => { inst.open = false; };
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
