/************************************Copyright*****************************************
 *  Project    : KNet
 *  Web        : https://github.com/1426186059/KNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/07/28 00:39:11
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using KNet.Common;
using KNet.LinuxTcp.Common;

namespace KNet.LinuxTcp.Server
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
            bool bSuccess = mNetServer.GetCryptoMgr().Decode(mReceiveStreamList, mNetPackage);
            if (bSuccess)
            {
                NetPackageExecute(mNetPackage);
            }
            return bSuccess;
        }
    }
}
