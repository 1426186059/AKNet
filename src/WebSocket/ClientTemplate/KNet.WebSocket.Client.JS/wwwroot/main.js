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
    // V2
    knet: {
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
    }
});

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

// 把 C# 侧的 XOR key 注入 V1 JS，使帧编解码与 KNet.Common 一致
try {
    const keyBytes = exports.KNet.WebSocket.Client.KNetJsBridge.GetXorKeyBytes();
    knetNet.setXorKey(keyBytes);
} catch (e) {
    console.error('[main] failed to set XOR key', e);
}

// 暴露给 Program.cs 的 [JSExport] 面板方法
window.knetExports = exports;

// run the C# Main() method and keep the runtime process running and executing further API calls
await runMain();
