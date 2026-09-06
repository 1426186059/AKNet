using Microsoft.AspNetCore.SignalR;
using KNet.WebSocketServer.SignalRSample;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
var app = builder.Build();

app.MapGet("/", () => "KNet SignalR Sample Server (.NET 10 / Kestrel). Connect a SignalR client to http://<host>:5271/hub");
app.MapHub<ChatHub>("/hub");

app.Run("http://localhost:5271");
