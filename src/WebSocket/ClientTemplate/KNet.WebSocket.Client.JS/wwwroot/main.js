// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import { dotnet } from './_framework/dotnet.js'
import * as knetWs from './jsengine/V2/knet-websocket.js';
import * as knetNet from './jsengine/V1/knet-net-client.js';

const { setModuleImports, getAssemblyExports, getConfig, runMain } = await dotnet
    .withApplicationArguments("start")
    .create();

// 注册两套导入：
//   knet.ws.*   -> V2 (C# 厚封装，JS 仅 WebSocket 原语)
//   knet.net.*  -> V1 (JS 厚封装，完整 KNet 客户端)
setModuleImports('main.js', {
    dom: {
        setInnerText: (selector, time) => { const el = document.querySelector(selector); if (el) el.innerText = time; },
        appendLog: (selector, text) => {
            const el = document.querySelector(selector);
            if (el) { const p = document.createElement('div'); p.textContent = text; el.appendChild(p); el.scrollTop = el.scrollHeight; }
        }
    },
    
    knet: {

        // V2
        wsConnect: knetWs.wsConnect,
        wsClose: knetWs.wsClose,
        wsSend: knetWs.wsSend,
        wsGetState: knetWs.wsGetState,
        wsReceive: knetWs.wsReceive,

        // V1
        netConnect: knetNet.netConnect,
        netClose: knetNet.netClose,
        netSend: knetNet.netSend,
        netGetState: knetNet.netGetState,
        netReceive: knetNet.netReceive,
        setXorKey: knetNet.setXorKey,
        netResetStats: knetNet.netResetStats,
        netGetStats: knetNet.netGetStats,
        netSendView: knetNet.netSendView,
        wsSendPointer: knetWs.wsSendPointer,
    }
});

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

// 深度遍历 exports 树：按“对象拥有指定函数”来定位，避免不同模板/版本下命名空间嵌套路径变化导致拿不到导出
function deepFindByMethod(obj, methodName, depth) {
    if (!obj || typeof obj !== 'object' || depth > 8) return null;
    for (const key of Object.keys(obj)) {
        const v = obj[key];
        if (v && typeof v === 'function' && key === methodName) return v;
        if (v && typeof v === 'object') {
            const found = deepFindByMethod(v, methodName, depth + 1);
            if (found) return found;
        }
    }
    return null;
}
function deepFindPerfPanel(obj, depth) {
    if (!obj || typeof obj !== 'object' || depth > 8) return null;
    for (const key of Object.keys(obj)) {
        const v = obj[key];
        if (v && typeof v === 'object' &&
            typeof v.SetVersion === 'function' &&
            typeof v.Connect === 'function' &&
            typeof v.Disconnect === 'function' &&
            typeof v.StressTest === 'function') {
            return v;
        }
        if (v && typeof v === 'object') {
            const found = deepFindPerfPanel(v, depth + 1);
            if (found) return found;
        }
    }
    return null;
}

// 暴露给 Program.cs 的 [JSExport] 面板方法
window.knetExports = exports;
// 显式路径优先，失败再深度遍历，彻底消除“命名空间回退脆弱”导致按钮没反应的问题
window.perfPanel = (exports && exports.KNet && exports.KNet.WebSocket && exports.KNet.WebSocket.Client && exports.KNet.WebSocket.Client.PerfPanel)
    || deepFindPerfPanel(exports, 0)
    || null;
console.log('[main] exports 顶层键:', Object.keys(exports));
if (!window.perfPanel) {
    console.error('[main] 未找到 PerfPanel 导出！请确认：1) Program.cs 中 [JSExport] 方法已生效；2) 浏览器已重新构建部署。');
}

// 把 C# 侧的 XOR key 注入 V1 JS，使帧编解码与 KNet.Common 一致
try {
    const getXorKeyBytes = deepFindByMethod(exports, 'GetXorKeyBytes', 0);
    if (getXorKeyBytes) knetNet.setXorKey(getXorKeyBytes());
    else console.warn('[main] 未找到 GetXorKeyBytes，V1 XOR key 未注入（V1 回显可能异常）');
} catch (e) {
    console.error('[main] failed to set XOR key', e);
}

// run the C# Main() method and keep the runtime process running and executing further API calls
await runMain();
