# AKNet

一个高性能 C# 网络库，核心目标是实现超越 TCP 的可靠有序 UDP 算法。支持 TCP / UDP / WebSocket / QUIC，协议间可无缝切换，服务端和客户端共用一套接口。

> **注意**：原 AKNet 仓库（[https://github.com/825126369/AKNet](https://github.com/825126369/AKNet)）已不再维护，此仓库为最新地址。

## 特点

- **TCP / UDP 无缝切换**：改一行参数即可在 TCP 和 UDP 之间切换，上层业务逻辑不用动。
- **超越 TCP 的可靠有序 UDP**：自研可靠有序算法，在 UDP 上实现了比 TCP 更灵活的传输控制。
- **内置 WebSocket 和 QUIC**：WebSocket 可用于 WebGL/网页端，QUIC 基于 MsQuic 做了 Demo 级实现。
- **适合学习**：代码结构清晰，模块化拆分，方便阅读和调试网络底层细节。比如把 Linux TCP 拥塞控制算法直译成了 C# 版本，对照内核源码看很容易理解。
- **可作为服务器网络网关的基石**：统一了多种协议的 Client/Server 模型，可以在此基础上搭建游戏网关、代理、转发等服务。

## 协议支持

| 协议 | NetType | 说明 |
|------|---------|------|
| UDP (可靠有序) | `Udp3Tcp` | 常用，自带可靠有序算法 |
| TCP | `TCP` | 标准 TCP 长连接 |
| WebSocket | `WebSocket` | WebGL / 浏览器端 |
| QUIC | `MSQuic` / `Quic` | MsQuic 移植，Demo 阶段 |
| LinuxUDP | `LinuxTCP` | Linux TCP 拥塞控制算法的 C# 移植 |

切换协议只需改构造函数参数：

```csharp
var server = new NetServerMain(NetType.Udp3Tcp);  // UDP
var server = new NetServerMain(NetType.TCP);       // TCP
var server = new NetServerMain(NetType.WebSocket); // WebSocket
```

## 快速开始

### Server

```csharp
using AKNet.Common;

public class NetServerHandler
{
    NetServerMain mNetServer;
    const int COMMAND_CHAT = 1000;

    public void Init()
    {
        var config = new ConfigInstance();
        config.MaxPlayerCount = 10;

        mNetServer = new NetServerMain(NetType.Udp3Tcp, config);
        mNetServer.addNetListenFunc(COMMAND_CHAT, OnReceiveMsg);
        mNetServer.InitNet(6000);
    }

    public void Update() => mNetServer.Update();

    void OnReceiveMsg(ClientPeerBase peer, NetPackage package)
    {
        Console.WriteLine($"收到消息, 长度: {package.GetData().Length}");
        peer.SendNetData(COMMAND_CHAT, package.GetData());
    }
}
```

### Client

```csharp
using AKNet.Common;

public class NetClientHandler
{
    NetClientMain mNetClient;
    const int COMMAND_CHAT = 1000;

    public void Init()
    {
        var config = new ConfigInstance();
        config.bAutoReConnect = true;

        mNetClient = new NetClientMain(NetType.Udp3Tcp, config);
        mNetClient.addListenClientPeerStateFunc(OnStateChanged);
        mNetClient.addNetListenFunc(COMMAND_CHAT, OnReceiveMsg);
        mNetClient.ConnectServer("127.0.0.1", 6000);
    }

    public void Update() => mNetClient.Update();

    void OnStateChanged(ClientPeerBase peer, SOCKET_PEER_STATE state)
    {
        Console.WriteLine($"连接状态: {state}");
    }

    void OnReceiveMsg(ClientPeerBase peer, NetPackage package)
    {
        Console.WriteLine($"收到回复, 长度: {package.GetData().Length}");
    }
}
```

### 入口

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

> 找份工作 30K 左右：10 多年 Unity 游戏开发经验
> 邮箱：1426186059@qq.com | 微信：AAA-2025-666-888

---

## Unity 中使用

把编译好的 DLL 放到 `Assets/Plugins/`，每帧调用 `Update()` 即可。

```csharp
public class NetworkManager : MonoBehaviour
{
    NetClientMain mNetClient;

    void Start()
    {
        mNetClient = new NetClientMain(NetType.WebSocket);
        mNetClient.ConnectServer("127.0.0.1", 6000);
    }

    void Update() => mNetClient.Update();
}
```

## 项目结构

```
src/
├── AKNet.Common/       # 公共接口、Buffer 管理、日志
├── AKNet/              # UDP/TCP 各协议模块
├── AKNet.WebSocket/    # WebSocket 实现
├── AKNet.Quic/         # QUIC 实现
├── AKNet.LinuxTcp/     # Linux TCP 拥塞控制算法 C# 移植
└── Test/               # 测试工程
```

## 版权声明

作者保留所有版权权利。**个人学习、研究、非商业用途可自由使用**。如需用于商业项目，请联系作者获取商业授权并支付版权费用。
