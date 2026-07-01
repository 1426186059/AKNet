using AKNet.Common;

namespace githubExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            NetLog.AddConsoleLog();
            var mServer = new NetServerHandler();
            mServer.Init();
            var mClient = new NetClientHandler();
            mClient.Init();

            while (true)
            {
                mServer.Update();
                mClient.Update();
                Thread.Sleep(1);
            }

            // 程序退出时清理资源
            // mClient.Dispose();
            // mServer.Dispose();
        }
    }
}
