/************************************Copyright*****************************************
 *  Project    : AKNet
 *  Web        : https://github.com/1426186059/AKNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/07/02 18:05:45
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using AKNet.Common;
using System;

namespace AKNet.Udp4Tcp.Server
{
    internal partial class ClientPeer
    {
        private void MultiThreadingReceiveStream(ReadOnlySpan<byte> e)
        {
            lock (mReceiveStreamList)
            {
                mReceiveStreamList.WriteFrom(e);
            }
        }

        private bool NetPackageExecute()
        {
            NetStreamReceivePackage mNetStreamPackage = mServerMgr.GetNetStreamPackage();
            bool bSuccess = false;
            lock (mReceiveStreamList)
            {
                bSuccess = mServerMgr.GetCryptoMgr().Decode(mReceiveStreamList, mNetStreamPackage);
            }

            if (bSuccess)
            {
                if (CommonTcpLayerNetCommand.orInnerCommand(mNetStreamPackage.nPackageId))
                {

                }
                else
                {
                    mServerMgr.GetPackageManager().NetPackageExecute(mWrap, mNetStreamPackage);
                }
            }

            return bSuccess;
        }

    }
}