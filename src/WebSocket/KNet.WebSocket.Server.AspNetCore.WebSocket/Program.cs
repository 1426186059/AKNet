using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 原生 WebSocket 中间件（Kestrel 内置，零额外依赖）
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});

app.MapGet("/", () => "KNet WebSocket Sample Server (.NET 10 / Kestrel). Connect a WebSocket client to ws://<host>:5270/ws");

// 回显（Echo）WebSocket 端点
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
    {
        var ws = await context.WebSockets.AcceptWebSocketAsync("echo");
        await EchoLoop(ws);
        return;
    }

    await next(context);
});

app.Run("http://localhost:5270");

static async Task EchoLoop(WebSocket ws)
{
    var buffer = new byte[4 * 1024];
    try
    {
        while (ws.State == WebSocketState.Open)
        {
            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
                break;
            }

            // 回显收到的内容（保留消息类型与边界）
            await ws.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count),
                result.MessageType, result.EndOfMessage, CancellationToken.None);
        }
    }
    catch (WebSocketException)
    {
        // 客户端异常断开，忽略
    }
}
