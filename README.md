# AKNet

一个高性能 C# 游戏网络库，支持 **UDP (可靠有序)** / **TCP** / **WebSocket** / **QUIC** / **LinuxUDP** 五种协议无缝切换，兼容 **Unity** 与 **.NET 8.0+**。

> 核心目标：UDP 超越 TCP 的可靠有序高性能算法。UDP 和 TCP 可一行代码切换，业务层完全无感。

---

## 支持的协议

| 协议 | NetType | 说明 |
|------|---------|------|
| UDP (Udp3Tcp) | `NetType.Udp3Tcp` | **推荐**，可靠有序 UDP，适合游戏帧同步/状态同步 |
| UDP (Udp1/2/4/5Tcp) | `NetType.Udp1Tcp` ~ `Udp5Tcp` | 不同算法变体，按需选用 |
| TCP | `NetType.TCP` | 标准 TCP，有状态长连接 |
| WebSocket | `NetType.WebSocket` | Unity WebGL / 网页端首选 |
| QUIC | `NetType.MSQuic` / `NetType.Quic` | 基于 MsQuic 的多路复用 Demo |
| LinuxUDP | `NetType.LinuxTCP` | C# 版 Linux TCP 拥塞控制算法移植 |

**一行代码切换协议：**

```csharp
// 只需改这一个参数
mNetServer = new NetServerMain(NetType.Udp3Tcp);   // UDP
mNetServer = new NetServerMain(NetType.TCP);        // TCP
mNetServer = new NetServerMain(NetType.WebSocket);  // WebSocket
```

---

## 快速开始

### NuGet 安装

```bash
dotnet add package AKNet
dotnet add package AKNet.Extentions.Protobuf   # Protobuf 扩展（可选）
```

### Server 端

```csharp
using AKNet.Common;

public class NetServerHandler
{
    NetServerMain mNetServer;
    const int COMMAND_TESTCHAT = 1000;

    public void Init()
    {
        ConfigInstance mConfig = new ConfigInstance();
        mConfig.bAutoReConnect = false;
        mConfig.MaxPlayerCount = 10;

        mNetServer = new NetServerMain(NetType.Udp3Tcp, mConfig);
        mNetServer.addNetListenFunc(COMMAND_TESTCHAT, OnReceiveMsg);
        mNetServer.InitNet(6000);
    }

    public void Update()
    {
        mNetServer.Update();
    }

    static void OnReceiveMsg(ClientPeerBase peer, NetPackage package)
    {
        Console.WriteLine($"收到消息，命令ID: {package.GetPackageId()}, 长度: {package.GetData().Length}");

        // Echo：原路返回
        peer.SendNetData(COMMAND_TESTCHAT, package.GetData());
    }
}
```

### Client 端

```csharp
using AKNet.Common;

public class NetClientHandler
{
    NetClientMain mNetClient;
    const int COMMAND_TESTCHAT = 1000;

    public void Init()
    {
        ConfigInstance mConfig = new ConfigInstance();
        mConfig.bAutoReConnect = true;

        mNetClient = new NetClientMain(NetType.Udp3Tcp, mConfig);
        mNetClient.addListenClientPeerStateFunc(OnStateChanged);
        mNetClient.addNetListenFunc(COMMAND_TESTCHAT, OnReceiveMsg);
        mNetClient.ConnectServer("127.0.0.1", 6000);
    }

    public void Update()
    {
        mNetClient.Update();
    }

    // 连接状态变更回调
    void OnStateChanged(ClientPeerBase peer, SOCKET_PEER_STATE state)
    {
        Console.WriteLine($"连接状态: {state}");
    }

    // 收到服务器消息
    void OnReceiveMsg(ClientPeerBase peer, NetPackage package)
    {
        Console.WriteLine($"收到回复，长度: {package.GetData().Length}");
    }
}
```

### Main 入口

```csharp
NetLog.AddConsoleLog();

var server = new NetServerHandler();
server.Init();

var client = new NetClientHandler();
client.Init();

while (true)
{
    server.Update();
    client.Update();
    Thread.Sleep(1);
}
```

---

## Protobuf 高级用法（可选）

配合 `AKNet.Extentions.Protobuf` 可直接发送 Protobuf 对象：

```csharp
using AKNet.Extentions.Protobuf;

// 发送 Protobuf 消息
mNetClient.SendNetData(COMMAND_TESTCHAT, chatMessage);

// 接收端直接解析
void OnReceiveMsg(ClientPeerBase peer, NetPackage package)
{
    var msg = TESTChatMessage.Parser.ParseFrom(package.GetData());
    Console.WriteLine(msg.TalkMsg);
}
```

---

## Unity 集成

1. 将 `AKNet.Common` / `AKNet` / `AKNet.WebSocket` 等 DLL 放入 `Assets/Plugins/`
2. 每帧调用 `Update()`

```csharp
public class NetworkManager : MonoBehaviour
{
    NetClientMain mNetClient;

    void Start()
    {
        mNetClient = new NetClientMain(NetType.WebSocket);
        mNetClient.ConnectServer("127.0.0.1", 6000);
    }

    void Update()
    {
        mNetClient.Update();
    }
}
```

---

## AKNet 的特点

- **UDP & TCP 无缝切换** — 一行代码切换协议，业务逻辑零改动
- **可靠有序 UDP** — 实现了超越 TCP 的可靠有序 UDP 算法
- **C# 版 Linux TCP** — 完整保留 Linux TCP 拥塞控制最精华的代码，可轻松同步上游
- **C# 版 MsQuic** — 保留 MsQuic 核心逻辑 Demo 阶段（`NetType.MSQuic` / `NetType.Quic`）
- **高性能** — 支持 IOCP / 对象池 / 零拷贝 Buffer
- **跨平台** — 支持 Unity (Windows/Mac/Linux/WebGL) + .NET 8.0+

## 网络协议对比

| 特性 | TCP | UDP (AKNet) | WebSocket | QUIC |
|------|-----|-------------|-----------|------|
| 可靠性 | ✅ 内置 | ✅ 算法保证 | ✅ 基于 TCP | ✅ 内置 |
| 有序性 | ✅ 内置 | ✅ 算法保证 | ✅ 基于 TCP | ✅ 基于 Stream |
| 速度 | 内核态 | 用户态可定制 | 基于 TCP | 用户态可定制 |
| 适用平台 | 全平台 | 全平台 | WebGL/浏览器 | Windows (.NET 8+) |
| 多路复用 | ❌ | ❌ | ❌ | ✅ |

## License

[MIT](LICENSE)

---

> 找份工作 30K 左右：10 多年 Unity 游戏开发经验
> 邮箱：1426186059@qq.com | 微信：AAA-2025-666-888
