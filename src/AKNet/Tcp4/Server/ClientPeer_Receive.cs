/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库 - 原生Socket async/await 服务端
*        Author:许珂
*        CreateTime:2026/6/28
*        Copyright:MIT软件许可证
************************************Copyright*****************************************/
using AKNet.Common;

namespace AKNet.Tcp4.Server
{
    internal partial class ClientPeer
    {
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
                    mServerMgr.mPackageManager.NetPackageExecute(mWrap, mNetPackage);
                }
            }

            return bSuccess;
        }
    }
}
