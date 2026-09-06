using Microsoft.AspNetCore.SignalR;

namespace KNet.WebSocketServer.SignalRSample;

public class ChatHub : Hub
{
    // 客户端调用：向所有连接广播一条消息
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Others.SendAsync("ReceiveMessage", "system", $"{Context.ConnectionId} connected");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.Others.SendAsync("ReceiveMessage", "system", $"{Context.ConnectionId} disconnected");
        await base.OnDisconnectedAsync(exception);
    }
}
