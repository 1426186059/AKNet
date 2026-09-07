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
using KNet.Common;
using System;
using System.Net;
using System.Runtime.InteropServices.JavaScript;

namespace KNet.WebSocket.Client
{
    /// <summary>
    /// V1（JS 厚封装）C# 薄包装。
    /// JS（knet-net-client.js）实现 WebSocket + XOR 帧编解码 + 心跳 + 消息队列；
    /// C# 只做 JSImport 调用：连接 / 发包 / 轮询取已解帧的包 / 状态查询。
    /// 包监听仍走 KNet.Common.ListenNetPackageMgr（与 V2 相同的回调接口）。
    /// </summary>
    public partial class NetClientMainJS
    {
        #region JS Interop (对应 wwwroot/jsengine/knet-net-client.js 的 knet.net*)

        [JSImport("knet.netConnect", "main.js")]
        private static partial int NetConnect(string url);

        [JSImport("knet.netClose", "main.js")]
        private static partial void NetClose(int instanceId);

        [JSImport("knet.netSend", "main.js")]
        private static partial int NetSend(int instanceId, int packageId, byte[] data);

        [JSImport("knet.netGetState", "main.js")]
        private static partial int NetGetState(int instanceId);

        [JSImport("knet.netReceive", "main.js")]
        private static partial byte[] NetReceive(int instanceId);

        [JSImport("knet.netResetStats", "main.js")]
        private static partial void NetResetStats();

        [JSImport("knet.netGetStats", "main.js")]
        private static partial string NetGetStats();

        // 零拷贝发送：C# 以 MemoryView（指针 + 长度）方式把原始 payload 直接交给 JS，
        // 避免 byte[] → Uint8Array 的 marshalling 拷贝。JS 侧读取后编码成帧并发送。
        // 零拷贝发送：Span<byte> 经 MemoryView 直接传 wasm 线性内存指针，JS 侧建 Uint8Array 视图后 ws.send。
        // 非 byte 类型（如结构体缓冲）可用 MemoryMarshal.AsBytes(structSpan) 先映射成 Span<byte>。
        [JSImport("knet.netSendView", "main.js")]
        private static partial int NetSendView(int instanceId, int packageId, [JSMarshalAs<JSType.MemoryView>] Span<byte> data);

        #endregion

        private readonly ListenNetPackageMgr mPackageManager;
        private readonly ListenClientPeerStateMgr mListenClientPeerStateMgr;
        private readonly ConfigInstance mConfigInstance;
        private readonly NetStreamReceivePackage mNetPackage = new NetStreamReceivePackage();

        private int mInstanceId = -1;
        private bool mZeroCopySend;
        private SOCKET_PEER_STATE mSocketPeerState;
        private SOCKET_PEER_STATE mLastSocketPeerState;
        private string mName = string.Empty;
        private uint mID = 0;
        private object mOwner = null;
        private string ServerIp = "";
        private int nServerPort = 0;
        private double fReConnectServerCdTime = 0.0;

        public NetClientMainJS(ConfigInstance mConfig = null, bool zeroCopySend = false)
        {
            mConfigInstance = mConfig ?? new ConfigInstance();
            mZeroCopySend = zeroCopySend;
            mPackageManager = new ListenNetPackageMgr();
            mListenClientPeerStateMgr = new ListenClientPeerStateMgr();
            mSocketPeerState = mLastSocketPeerState = SOCKET_PEER_STATE.DISCONNECTED;
        }

        public void ConnectServer(string Ip, int nPort)
        {
            MainThreadCheck.Check();
            Reset();
            this.ServerIp = Ip;
            this.nServerPort = nPort;
            SetSocketState(SOCKET_PEER_STATE.CONNECTING);

            var uri = $"ws://{Ip}:{nPort}/";
            NetLog.Log($"WebSocket(V1) 正在连接服务器: {uri}");

            mInstanceId = NetConnect(uri);
            if (mInstanceId <= 0)
            {
                NetLog.LogError($"WebSocket(V1) 连接服务器失败: {uri}");
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            }
        }

        public bool DisConnectServer()
        {
            NetLog.Log("WebSocket(V1) 主动断开服务器 Begin......");
            MainThreadCheck.Check();

            if (mInstanceId > 0)
            {
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTING);
                NetClose(mInstanceId);
                mInstanceId = -1;
            }
            SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            NetLog.Log("WebSocket(V1) 主动断开服务器 Finish......");
            return true;
        }

        public void ReConnectServer()
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED) return;
            ConnectServer(this.ServerIp, this.nServerPort);
        }

        public void Update() { }

        public void Update(double elapsed)
        {
            if (mInstanceId > 0)
            {
                int state = NetGetState(mInstanceId);
                SOCKET_PEER_STATE cur = GetSocketState();

                if (state == 1 && cur != SOCKET_PEER_STATE.CONNECTED)
                {
                    NetLog.Log($"WebSocket(V1) 连接成功, state={state}");
                    SetSocketState(SOCKET_PEER_STATE.CONNECTED);
                }
                else if (state >= 2 && cur != SOCKET_PEER_STATE.DISCONNECTED &&
                         cur != SOCKET_PEER_STATE.DISCONNECTING &&
                         cur != SOCKET_PEER_STATE.RECONNECTING)
                {
                    NetLog.Log($"WebSocket(V1) 连接关闭/错误, state={state}");
                    DisConnectedWithError();
                }

                // 轮询已解帧的包，分发给监听器
                while (true)
                {
                    byte[] pkt = NetReceive(mInstanceId);
                    if (pkt == null || pkt.Length < 2) break;

                    ushort packageId = (ushort)((pkt[0] << 8) | pkt[1]);
                    byte[] body = new byte[pkt.Length - 2];
                    if (body.Length > 0)
                        Buffer.BlockCopy(pkt, 2, body, 0, body.Length);

                    mNetPackage.nPackageId = packageId;
                    mNetPackage.SetData(new Memory<byte>(body));

                    if (CommonTcpLayerNetCommand.orInnerCommand(packageId)) { }
                    else mPackageManager.NetPackageExecute(this, mNetPackage);
                }
            }

            // 重连倒计时（心跳由 JS 处理）
            if (GetSocketState() == SOCKET_PEER_STATE.RECONNECTING)
            {
                fReConnectServerCdTime += elapsed;
                if (fReConnectServerCdTime >= CommonTcpLayerConfig.fReConnectMaxCdTime)
                {
                    fReConnectServerCdTime = 0.0;
                    ReConnectServer();
                }
            }

            if (mSocketPeerState != mLastSocketPeerState)
            {
                mLastSocketPeerState = mSocketPeerState;
                mListenClientPeerStateMgr.OnSocketStateChanged(this);
            }
        }

        private void DisConnectedWithError()
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTING)
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            else if (GetSocketState() == SOCKET_PEER_STATE.DISCONNECTING)
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            else if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
            {
                if (mConfigInstance.bAutoReConnect)
                    SetSocketState(SOCKET_PEER_STATE.RECONNECTING);
                else
                    SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            }
        }

        private void Reset()
        {
            SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            if (mInstanceId > 0) { NetClose(mInstanceId); mInstanceId = -1; }
            fReConnectServerCdTime = 0.0;
        }

        public void Dispose() => Reset();

        // 诊断：V1 全局收发统计（供压测定位丢包在发送侧还是接收侧）
        public static void ResetV1Stats() => NetResetStats();
        public static (long sendOk, long sendFail, long decoded, long recvEvents) GetV1Stats()
        {
            var parts = (NetGetStats() ?? "").Split(',');
            long TryGet(int i) => parts.Length > i && long.TryParse(parts[i], out var v) ? v : 0;
            return (TryGet(0), TryGet(1), TryGet(2), TryGet(3));
        }

        #region Send

        public void SendNetData(ushort nPackageId)
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
                NetSend(mInstanceId, nPackageId, Array.Empty<byte>());
            else NetLog.LogError("SendNetData Failed: " + GetSocketState());
        }

        public void SendNetData(ushort nPackageId, byte[] data)
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
                SendCore(nPackageId, data ?? Array.Empty<byte>());
            else NetLog.LogError("SendNetData Failed: " + GetSocketState());
        }

        // 发送核心：按发送策略选择「MemoryView 指针零拷贝 / byte[] 默认拷贝」路径。
        private unsafe void SendCore(ushort nPackageId, byte[] data)
        {
            int r;
            if (mZeroCopySend)
            {
                r = NetSendView(mInstanceId, nPackageId, data.AsSpan());   // 零拷贝：byte[] → Span<byte> 视图
            }
            else
            {
                r = NetSend(mInstanceId, nPackageId, data);
            }
            if (r == 0) NetLog.LogWarning("WebSocket(V1) 发送失败，连接可能已断开");
        }

        public void SendNetData(byte[] data) => SendNetData(0, data);

        #endregion

        #region Listeners & State

        public void addNetListenFunc(ushort nPackageId, Action<ClientPeerBase, NetPackage> fun)
        { mPackageManager.addNetListenFunc(nPackageId, fun); }
        public void removeNetListenFunc(ushort nPackageId, Action<ClientPeerBase, NetPackage> fun)
        { mPackageManager.removeNetListenFunc(nPackageId, fun); }
        public void addNetListenFunc(Action<ClientPeerBase, NetPackage> func)
        { mPackageManager.addNetListenFunc(func); }
        public void removeNetListenFunc(Action<ClientPeerBase, NetPackage> func)
        { mPackageManager.removeNetListenFunc(func); }

        public void addListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> mFunc)
        { mListenClientPeerStateMgr.addListenClientPeerStateFunc(mFunc); }
        public void removeListenClientPeerStateFunc(Action<ClientPeerBase, SOCKET_PEER_STATE> mFunc)
        { mListenClientPeerStateMgr.removeListenClientPeerStateFunc(mFunc); }
        public void addListenClientPeerStateFunc(Action<ClientPeerBase> mFunc)
        { mListenClientPeerStateMgr.addListenClientPeerStateFunc(mFunc); }
        public void removeListenClientPeerStateFunc(Action<ClientPeerBase> mFunc)
        { mListenClientPeerStateMgr.removeListenClientPeerStateFunc(mFunc); }

        public IPEndPoint GetIPEndPoint() => null;
        public SOCKET_PEER_STATE GetSocketState() => mSocketPeerState;
        private void SetSocketState(SOCKET_PEER_STATE s) => mSocketPeerState = s;

        public void SetName(string name) { mName = name; }
        public string GetName() => mName;
        public void SetID(uint id) { mID = id; }
        public uint GetID() => mID;
        public void SetOwner(object owner) { mOwner = owner; }
        public object GetOwner() => mOwner;

        #endregion
    }
}
