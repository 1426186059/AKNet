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
using KNet.Udp2Tcp.Common;

namespace KNet.Udp2Tcp.Server
{
    internal partial class ClientPeer
    {
        private bool GetReceiveCheckPackage()
        {
            NetUdpFixedSizePackage mPackage = null;
            if (GetReceivePackage(out mPackage))
            {
                UdpStatistical.AddReceivePackageCount();
                NetLog.Assert(mPackage != null, "mPackage == null");
                mUdpCheckPool.ReceiveNetPackage(mPackage);
                return true;
            }
            return false;
        }

        public void ReceiveTcpStream(NetUdpFixedSizePackage mPackage)
        {
            mReceiveStreamList.WriteFrom(mPackage.GetTcpBufferSpan());
        }

        private bool NetTcpPackageExecute()
        {
            var mNetPackage = mServerMgr.GetLikeTcpNetPackage();
            bool bSuccess = mServerMgr.GetCryptoMgr().Decode(mReceiveStreamList, mNetPackage);
            if (bSuccess)
            {
                NetPackageExecute(mNetPackage);
            }
            return bSuccess;
        }

    }
}