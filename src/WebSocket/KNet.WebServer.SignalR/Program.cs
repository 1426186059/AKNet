using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5271");
builder.Services.AddSignalR();
var app = builder.Build();

app.MapHub<EchoHub>("/hub");

app.Run();
