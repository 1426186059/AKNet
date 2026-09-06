using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.SignalR.Client;

var wsUrl = "ws://localhost:5270/ws";
var hubUrl = "http://localhost:5271/hub";

Console.WriteLine("=== KNet.WebSocketTest: 连接两个 ASP.NET Core Web 服务器 (.NET 10) ===");

await TestWebSocket(wsUrl);
await TestSignalR(hubUrl);

Console.WriteLine("=== 完成 (按任意键退出) ===");
Console.ReadKey();

static async Task TestWebSocket(string url)
{
    Console.WriteLine($"[WebSocket] 连接 {url}");
    using var client = new ClientWebSocket();
    await client.ConnectAsync(new Uri(url), CancellationToken.None);

    var payload = Encoding.UTF8.GetBytes("hello from KNet test client");
    await client.SendAsync(payload, WebSocketMessageType.Text, true, CancellationToken.None);

    var buffer = new byte[4 * 1024];
    var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    Console.WriteLine($"[WebSocket] 收到回显: {Encoding.UTF8.GetString(buffer, 0, result.Count)}");

    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
    Console.WriteLine("[WebSocket] 已关闭");
}

static async Task TestSignalR(string url)
{
    Console.WriteLine($"[SignalR] 连接 {url}");
    var connection = new HubConnectionBuilder().WithUrl(url).Build();
    connection.On<string, string>("ReceiveMessage", (user, message) =>
        Console.WriteLine($"[SignalR] 收到广播: {user}: {message}"));

    await connection.StartAsync();
    Console.WriteLine("[SignalR] 已连接，调用 SendMessage");
    await connection.InvokeAsync("SendMessage", "tester", "hello signalr");

    await Task.Delay(500);
    await connection.StopAsync();
    Console.WriteLine("[SignalR] 已停止");
}
