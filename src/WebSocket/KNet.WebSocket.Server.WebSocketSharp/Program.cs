// WebSocketSharp 版 WebSocket 服务器入口：创建 NetServerMain，注册回显，启动后阻塞等待 Ctrl+C。
using KNet.Common;
using System;
using System.Threading;

namespace KNet.WebSocket.Server
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            int port = ParsePort(args, 9002);

            NetLog.AddConsoleLog();

            var server = new NetServerMain();

            server.addNetListenFunc((ClientPeerBase peer, NetPackage pkg) =>
                peer.SendNetData(pkg.GetPackageId(), pkg.GetData().ToArray()));

            server.InitNet(port);

            NetLog.Log($"[WebSocketSharp] WebSocket 服务器已启动: ws://0.0.0.0:{port}  (Ctrl+C 退出)");

            var exit = new ManualResetEvent(false);
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; exit.Set(); };
            exit.WaitOne();

            server.Dispose();
            NetLog.Log("[WebSocketSharp] 服务器已停止");
        }

        private static int ParsePort(string[] args, int fallback)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if ((args[i] == "--port" || args[i] == "-p") && i + 1 < args.Length && int.TryParse(args[i + 1], out int p))
                    return p;
            }
            return fallback;
        }
    }
}
