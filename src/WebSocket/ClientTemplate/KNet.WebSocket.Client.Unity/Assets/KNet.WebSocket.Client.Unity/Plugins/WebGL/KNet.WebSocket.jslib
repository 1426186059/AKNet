/**
 * KNet.WebSocket - Unity WebGL JSLib
 *
 * 使用方法：将此文件放入 Unity 项目的 Assets/Plugins/WebGL/ 目录
 * 
 * 设计：使用轮询模式，JS 将消息放入队列，C# 在 Update() 中轮询取出
 * 避免 JS 回调线程与 Unity 主线程的同步问题
 */

mergeInto(LibraryManager.library, {

    AKWebSocket_Init: function() {
        AKWebSocket_Instances = AKWebSocket_Instances || {};
        AKWebSocket_MessageQueue = AKWebSocket_MessageQueue || {};
        AKWebSocket_EventQueue = AKWebSocket_EventQueue || {};
    },

    AKWebSocket_Connect: function(url, instanceId) {
        AKWebSocket_Init();
        var wsUrl = UTF8ToString(url);
        try {
            var ws = new WebSocket(wsUrl);
            ws.binaryType = 'arraybuffer';
            
            ws.onopen = function() {
                if (!AKWebSocket_EventQueue[instanceId]) {
                    AKWebSocket_EventQueue[instanceId] = [];
                }
                AKWebSocket_EventQueue[instanceId].push(1); // Opened
            };
            
            ws.onmessage = function(event) {
                var data = new Uint8Array(event.data);
                if (!AKWebSocket_MessageQueue[instanceId]) {
                    AKWebSocket_MessageQueue[instanceId] = [];
                }
                AKWebSocket_MessageQueue[instanceId].push(data);
            };
            
            ws.onclose = function(event) {
                if (!AKWebSocket_EventQueue[instanceId]) {
                    AKWebSocket_EventQueue[instanceId] = [];
                }
                AKWebSocket_EventQueue[instanceId].push(2); // Closed
                delete AKWebSocket_Instances[instanceId];
            };
            
            ws.onerror = function(event) {
                if (!AKWebSocket_EventQueue[instanceId]) {
                    AKWebSocket_EventQueue[instanceId] = [];
                }
                AKWebSocket_EventQueue[instanceId].push(3); // Error
                delete AKWebSocket_Instances[instanceId];
            };
            
            AKWebSocket_Instances[instanceId] = ws;
            return 1;
        } catch(e) {
            console.error('AKWebSocket: Connect failed', e);
            return 0;
        }
    },

    AKWebSocket_Close: function(instanceId) {
        var ws = AKWebSocket_Instances ? AKWebSocket_Instances[instanceId] : null;
        if (ws) {
            try {
                ws.close(1000, 'Normal Closure');
            } catch(e) {}
            delete AKWebSocket_Instances[instanceId];
        }
    },

    AKWebSocket_Send: function(instanceId, dataPtr, dataLen) {
        var ws = AKWebSocket_Instances ? AKWebSocket_Instances[instanceId] : null;
        if (ws && ws.readyState === WebSocket.OPEN) {
            var data = HEAPU8.slice(dataPtr, dataPtr + dataLen);
            ws.send(data.buffer);
            return 1;
        }
        return 0;
    },

    AKWebSocket_GetReadyState: function(instanceId) {
        var ws = AKWebSocket_Instances ? AKWebSocket_Instances[instanceId] : null;
        return ws ? ws.readyState : 3;
    },

    AKWebSocket_PollEvent: function(instanceId) {
        // Returns: 0=无事件, 1=已打开, 2=已关闭, 3=错误
        if (!AKWebSocket_EventQueue || !AKWebSocket_EventQueue[instanceId]) {
            return 0;
        }
        var events = AKWebSocket_EventQueue[instanceId];
        if (events.length > 0) {
            return events.shift();
        }
        return 0;
    },

    AKWebSocket_PollMessageLength: function(instanceId) {
        // 返回待处理消息的总字节数，0=无消息
        if (!AKWebSocket_MessageQueue || !AKWebSocket_MessageQueue[instanceId]) {
            return 0;
        }
        var msgs = AKWebSocket_MessageQueue[instanceId];
        if (msgs.length > 0) {
            return msgs[0].length;
        }
        return 0;
    },

    AKWebSocket_PollMessageCopy: function(instanceId, destPtr, maxLen) {
        // 将消息数据拷贝到 C# 提供的缓冲区，返回实际拷贝字节数
        if (!AKWebSocket_MessageQueue || !AKWebSocket_MessageQueue[instanceId]) {
            return 0;
        }
        var msgs = AKWebSocket_MessageQueue[instanceId];
        if (msgs.length > 0) {
            var data = msgs.shift();
            var copyLen = Math.min(data.length, maxLen);
            HEAPU8.set(data.subarray(0, copyLen), destPtr);
            return copyLen;
        }
        return 0;
    }
});
