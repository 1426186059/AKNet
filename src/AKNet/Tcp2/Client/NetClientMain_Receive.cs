/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库 - TcpClient 实现
*        Author:许珂
*        CreateTime:2026/6/28
*        Copyright:MIT软件许可证
************************************Copyright*****************************************/
using AKNet.Common;

namespace AKNet.Tcp2.Client
{
    internal partial class NetClientMain
    {
        private bool NetPackageExecute()
        {
            bool bSuccess = false;
            lock (mReceiveStreamList)
            {
                bSuccess = mCryptoMgr.Decode(mReceiveStreamList, mNetPackage);
            }

            if (bSuccess)
            {
                if (CommonTcpLayerNetCommand.orInnerCommand(mNetPackage.nPackageId))
                {

                }
                else
                {
                    mPackageManager.NetPackageExecute(this, mNetPackage);
                }
            }

            return bSuccess;
        }
    }
}
