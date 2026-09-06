using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using KNet.Common;

Console.WriteLine("Hello, Browser!");

if (args.Length == 1 && args[0] == "start")
    PerfPanel.Start();

while (true)
{
    PerfPanel.Tick();
    await Task.Delay(16);
}

/// <summary>
/// KNet 浏览器客户端性能测试面板。
/// 可切换 V1（JS 厚封装）/ V2（C# 厚封装），发起突发发送并统计往返/吞吐。
/// </summary>
internal static partial class PerfPanel
{
    private static NetClientInterface _client;
    private static NetType _version = NetType.WebSocketJS_V2;
    private static readonly Stopwatch _sw = new();

    // 统计
    private static long _sentPackets;
    private static long _sentBytes;
    private static long _recvPackets;
    private static long _recvBytes;
    private static long _lastSentPackets;
    private static long _lastRecvPackets;
    private static double _lastTime;

    // 高并发压测状态
    private static List<NetClientInterface> _stressClients = new List<NetClientInterface>();
    private static long _stressRecv;
    private static string _lastHost = "127.0.0.1";
    private static int _lastPort = 9000;

    public static void Start() => _sw.Start();

    [JSImport("dom.setInnerText", "main.js")]
    internal static partial void SetInnerText(string selector, string content);

    [JSImport("dom.appendLog", "main.js")]
    internal static partial void AppendLog(string selector, string text);

    /// <summary>切换版本（未连接时有效）。</summary>
    [JSExport]
    internal static void SetVersion(int v)
    {
        if (_client != null)
        {
            if (_client.GetSocketState() != SOCKET_PEER_STATE.DISCONNECTED)
            {
                AppendLog("#log", "已连接，无法切换版本。请先断开。");
                return;
            }
            _client.Dispose();
            _client = null;
        }
        _version = v == 1 ? NetType.WebSocketJS_V1 : NetType.WebSocketJS_V2;
        AppendLog("#log", $"版本切换为: {(_version == NetType.WebSocketJS_V1 ? "V1 (JS厚封装)" : "V2 (C#厚封装)")}");
    }

    [JSExport]
    internal static void Connect(string host, string portText)
    {
        if (_client != null)
        {
            if (_client.GetSocketState() == SOCKET_PEER_STATE.DISCONNECTED)
            {
                // 上次连接失败/已断开，释放后允许重新连接
                _client.Dispose();
                _client = null;
            }
            else
            {
                AppendLog("#log", "已连接，请先断开。");
                return;
            }
        }
        if (!int.TryParse(portText, out int port))
        {
            AppendLog("#log", "端口无效");
            return;
        }
        _lastHost = host;
        _lastPort = port;

        _client = new NetClientMain(_version).GetInstance();
        _client.addListenClientPeerStateFunc(peer =>
        {
            AppendLog("#log", $"状态变化: {peer.GetSocketState()}");
        });
        // 通用包监听，统计收包
        _client.addNetListenFunc((peer, pkg) =>
        {
            _recvPackets++;
            _recvBytes += pkg.GetData().Length;
        });

        _client.ConnectServer(host, port);
        AppendLog("#log", $"连接请求: {host}:{port} ({_version})");
    }

    [JSExport]
    internal static void Disconnect()
    {
        if (_client != null)
        {
            _client.DisConnectServer();
            _client.Dispose();
            _client = null;
            AppendLog("#log", "已断开");
        }
    }

    /// <summary>突发发送 count 个包，每包 bodySize 字节（packageId=1000）。</summary>
    [JSExport]
    internal static void BurstSend(int count, int bodySize)
    {
        if (_client == null || _client.GetSocketState() != SOCKET_PEER_STATE.CONNECTED)
        {
            AppendLog("#log", "未连接，无法发送");
            return;
        }

        byte[] body = new byte[bodySize];
        for (int i = 0; i < bodySize; i++) body[i] = (byte)(i & 0xff);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < count; i++)
        {
            _client.SendNetData(1000, body);
            _sentPackets++;
            _sentBytes += bodySize;
        }
        sw.Stop();
        AppendLog($"#log", $"突发发送 {count} 包 x {bodySize}B = {count * bodySize}B, 耗时 {sw.ElapsedMilliseconds}ms");
    }

    [JSExport]
    internal static void ClearLog()
    {
        SetInnerText("#log", "");
    }

    /// <summary>
    /// 高并发压测（类似 NetTestClientBase）：clientCount 个连接，每连接发送 perClient 包，
    /// 合计 clientCount×perClient 包（默认 100×10000 = 1,000,000），用 Stopwatch 统计发送耗时与回显吞吐。
    /// </summary>
    [JSExport]
    internal static async Task StressTest(int clientCount, int perClient)
    {
        if (clientCount <= 0 || perClient <= 0)
        {
            AppendLog("#log", "参数无效（连接数与每连接包数均需 > 0）");
            return;
        }
        StopStress();

        int total = clientCount * perClient;
        _stressRecv = 0;
        byte[] body = new byte[64];
        for (int i = 0; i < body.Length; i++) body[i] = (byte)(i & 0xff);

        AppendLog("#log", $"高并发压测开始: {clientCount} 连接 × {perClient} 包 = {total} 包 -> {_lastHost}:{_lastPort} ({_version})");

        // 1) 创建并连接所有客户端
        for (int i = 0; i < clientCount; i++)
        {
            var c = new NetClientMain(_version).GetInstance();
            c.addListenClientPeerStateFunc(p => { });
            c.addNetListenFunc((peer, pkg) => { _stressRecv++; });
            c.ConnectServer(_lastHost, _lastPort);
            _stressClients.Add(c);
        }

        // 2) 等待全部连接（让出事件循环以便 WebSocket 回调执行）
        int connected = 0;
        var connectSw = Stopwatch.StartNew();
        while (connected < clientCount && connectSw.ElapsedMilliseconds < 15000)
        {
            connected = 0;
            foreach (var c in _stressClients) { c.Update(0.016); if (c.GetSocketState() == SOCKET_PEER_STATE.CONNECTED) connected++; }
            await Task.Delay(16);
        }
        connectSw.Stop();
        AppendLog("#log", $"连接完成: {connected}/{clientCount} 已连接, 耗时 {connectSw.ElapsedMilliseconds}ms");
        if (connected < clientCount) AppendLog("#log", "警告: 部分连接未成功，压测指标仅供参考");

        // 3) 突发发送全部包并计时
        long sent = 0;
        var sendSw = Stopwatch.StartNew();
        foreach (var c in _stressClients)
        {
            if (c.GetSocketState() != SOCKET_PEER_STATE.CONNECTED) continue;
            for (int k = 0; k < perClient; k++)
            {
                c.SendNetData(1000, body);
                sent++;
            }
            c.Update(0.016);
        }
        for (int f = 0; f < 3; f++) { foreach (var c in _stressClients) c.Update(0.016); await Task.Delay(16); }
        sendSw.Stop();
        double sendSec = Math.Max(1, sendSw.ElapsedMilliseconds) / 1000.0;
        AppendLog("#log", $"发送完成: {sent} 包, 耗时 {sendSw.ElapsedMilliseconds}ms, 吞吐 {(sent / sendSec):F0} 包/s ({(sent * body.Length / sendSec / 1024.0):F1} KB/s)");

        // 4) 等待服务端回显（echo）全部到达
        var recvSw = Stopwatch.StartNew();
        while (_stressRecv < sent && recvSw.ElapsedMilliseconds < 30000)
        {
            foreach (var c in _stressClients) c.Update(0.016);
            await Task.Delay(16);
        }
        recvSw.Stop();
        AppendLog("#log", $"回显完成: 收到 {_stressRecv}/{sent} 包, 耗时 {recvSw.ElapsedMilliseconds}ms, 往返吞吐 {(_stressRecv / Math.Max(1, recvSw.ElapsedMilliseconds) * 1000.0):F0} 包/s");
        AppendLog("#log", $"高并发压测结束: 连接 {connected}, 发送 {sent} 包, 接收 {_stressRecv} 包");
    }

    [JSExport]
    internal static void StopStress()
    {
        foreach (var c in _stressClients) { try { c.DisConnectServer(); } catch { } try { c.Dispose(); } catch { } }
        _stressClients.Clear();
        _stressRecv = 0;
    }

    internal static void Tick()
    {
        if (_client != null)
        {
            _client.Update(0.016);
        }
        for (int i = 0; i < _stressClients.Count; i++) _stressClients[i].Update(0.016);

        double now = _sw.Elapsed.TotalSeconds;
        double dt = now - _lastTime;
        if (dt >= 1.0)
        {
            long sentRate = (long)((_sentPackets - _lastSentPackets) / dt);
            long recvRate = (long)((_recvPackets - _lastRecvPackets) / dt);
            _lastSentPackets = _sentPackets;
            _lastRecvPackets = _recvPackets;
            _lastTime = now;

            SetInnerText("#state", _client?.GetSocketState().ToString() ?? "未连接");
            SetInnerText("#sent", $"{_sentPackets} 包 / {_sentBytes} B  (发送 {sentRate}/s)");
            SetInnerText("#recv", $"{_recvPackets} 包 / {_recvBytes} B  (接收 {recvRate}/s)");
            SetInnerText("#uptime", _sw.Elapsed.ToString(@"hh\:mm\:ss"));
        }
    }
}
