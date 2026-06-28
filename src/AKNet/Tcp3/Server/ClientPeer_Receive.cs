/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库
*        Author:许珂
*        StartTime:2024/11/01 00:00:00
*        ModifyTime:2026/6/28 00:00:00
*        Copyright:MIT软件许可证
************************************Copyright*****************************************/
using AKNet.Common;
using System;

namespace AKNet.Tcp3.Server
{
    internal partial class ClientPeer
    {
        public void MultiThreadingReceiveSocketStream(int bytesRead)
        {
            lock (mReceiveStreamList)
            {
                mReceiveStreamList.WriteFrom(new ReadOnlySpan<byte>(mReceiveBuffer, 0, bytesRead));
            }
        }

        private bool NetPackageExecute()
        {
            NetStreamReceivePackage mNetPackage = mServerMgr.mNetPackage;
            bool bSuccess = false;
            lock (mReceiveStreamList)
            {
                bSuccess = mServerMgr.mCryptoMgr.Decode(mReceiveStreamList, mNetPackage);
            }

            if (bSuccess)
            {
                if (CommonTcpLayerNetCommand.orInnerCommand(mNetPackage.nPackageId))
                {

                }
                else
                {
                    mServerMgr.mPackageManager.NetPackageExecute(this, mNetPackage);
                }
            }

            return bSuccess;
        }
    }
}
