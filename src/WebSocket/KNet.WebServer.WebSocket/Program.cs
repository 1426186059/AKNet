using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5270");
var app = builder.Build();

app.UseWebSockets();

// 原生 WebSocket 中间件：路径 /ws，收到消息原样回显（支持分帧/多段消息）。
app.Map("/ws", async (HttpContext context) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    using var ws = await context.WebSockets.AcceptWebSocketAsync();
    await EchoLoopAsync(ws);
});

app.Run();

static async Task EchoLoopAsync(WebSocket ws)
{
    var buffer = new byte[4 * 1024];
    var segments = new List<byte[]>();
    while (ws.State == WebSocketState.Open)
    {
        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        if (result.MessageType == WebSocketMessageType.Close)
        {
            if (ws.State != WebSocketState.Closed)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
            }
            break;
        }

        var chunk = new byte[result.Count];
        Array.Copy(buffer, chunk, result.Count);
        segments.Add(chunk);

        if (result.EndOfMessage)
        {
            // 合并被分帧的消息后整体回显
            var message = new byte[segments.Sum(s => s.Length)];
            int offset = 0;
            foreach (var s in segments)
            {
                Array.Copy(s, 0, message, offset, s.Length);
                offset += s.Length;
            }
            segments.Clear();

            await ws.SendAsync(new ArraySegment<byte>(message), result.MessageType, true, CancellationToken.None);
        }
    }
}
