using KNet.Common;

namespace TestNetClient
{
    public class NetHandler : NetTestClientBase
    {
        public override NetClientInterface Create()
        {
            return new NetClientMain(NetType.WebSocket);
        }

        public override void OnTestFinish()
        {
            NetLog.Log("WebSocket 测试全部完成!");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var mTest = new NetHandler();
            mTest.Start();
        }
    }
}
