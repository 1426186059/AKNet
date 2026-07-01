/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库
*        Author:许珂
*        StartTime:2024/11/01 00:00:00
*        ModifyTime:2026/2/1 20:27:10
*        Copyright:MIT软件许可证
************************************Copyright*****************************************/
using AKNet.Common;
using AKNet.LinuxTcp.Common;

namespace AKNet.LinuxTcp.Server
{
    internal partial class ClientPeer
    {
        private void GetReceiveCheckPackage()
        {
            sk_buff mPackage = null;
            do
            {
                mPackage = GetReceivePackage();
                if (mPackage != null)
                {
                    mUdpCheckPool.ReceiveNetPackage(mPackage);
                }
            }
            while (mPackage != null);
        }

        private void ReceiveTcpStream()
        {
            while (mUdpCheckPool.ReceiveTcpStream(mTcpMsg))
            {
                while (NetTcpPackageExecute())
                {

                }
            }
        }

        private bool NetTcpPackageExecute()
        {
            var mNetPackage = mNetServer.GetLikeTcpNetPackage();
            bool bSuccess = mNetServer.mCryptoMgr.Decode(mReceiveStreamList, mNetPackage);
            if (bSuccess)
            {
                NetPackageExecute(mNetPackage);
            }
            return bSuccess;
        }
    }
}
