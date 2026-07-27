using KNet.Common;

namespace TestNetClient
{
    public class NetHandler : NetTestClientBase
    {
        NetType mNetType = NetType.Tcp4;
        public override NetClientMainBase Create()
        {
            return new NetClientMain(mNetType);
        }

        public override void OnTestFinish()
        {
            if (mNetType == NetType.Udp4Tcp)
            {
                KNet.Udp4Tcp.Common.UdpStatistical.PrintLog();
            }
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
