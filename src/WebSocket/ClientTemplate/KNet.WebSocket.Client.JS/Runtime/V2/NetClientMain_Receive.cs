/************************************Copyright*****************************************
 *  Project    : KNet
 *  Web        : https://github.com/1426186059/KNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/09/06 00:00:00
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using KNet.Common;
using System;

namespace KNet.WebSocket.Client
{
    public partial class NetClientMain
    {
        /// <summary>从接收环形缓冲解出一帧并分发包监听器。返回 true 表示成功解析一帧。</summary>
        private bool NetPackageExecute()
        {
            bool bSuccess = false;
            lock (mReceiveStreamList)
            {
                bSuccess = mCryptoMgr.Decode(mReceiveStreamList, mNetPackage);
            }
            if (bSuccess)
            {
                if (CommonTcpLayerNetCommand.orInnerCommand(mNetPackage.nPackageId)) { }
                else { mPackageManager.NetPackageExecute(this, mNetPackage); }
            }
            return bSuccess;
        }
    }
}
