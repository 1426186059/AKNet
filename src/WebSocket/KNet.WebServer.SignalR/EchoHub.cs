using Microsoft.AspNetCore.SignalR;

// 广播式 Echo Hub：客户端调用 SendMessage(user, message)，服务器向所有连接转发 ReceiveMessage(user, message)。
public class EchoHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
