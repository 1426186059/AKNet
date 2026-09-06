

using KNet.Common;
using KNet.WebSocket.Server;

namespace TestNetServer
{
    public class NetHandler : NetTestServerBase
    {
        public override NetServerInterface Create()
        {
            return new NetServerMain();
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
