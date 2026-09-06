// Fleck 版 WebSocket 服务器入口：创建 NetServerMain，注册回显，启动后阻塞等待 Ctrl+C。
using KNet.Common;
using System;
using System.Threading;

namespace KNet.WebSocket.Server
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            int port = ParsePort(args, 9001);

            NetLog.AddConsoleLog();

            var server = new NetServerMain();

            // 收到任意消息：原样回显（与 KNet 服务器行为一致，避免把原始字节当 protobuf 解析报错）
            server.addNetListenFunc((ClientPeerBase peer, NetPackage pkg) =>
                peer.SendNetData(pkg.GetPackageId(), pkg.GetData().ToArray()));

            server.InitNet(port);

            NetLog.Log($"[Fleck] WebSocket 服务器已启动: ws://0.0.0.0:{port}  (Ctrl+C 退出)");

            var exit = new ManualResetEvent(false);
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; exit.Set(); };
            exit.WaitOne();

            server.Dispose();
            NetLog.Log("[Fleck] 服务器已停止");
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
