using System;
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

    internal static void Tick()
    {
        if (_client != null)
        {
            _client.Update(0.016);
        }

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
