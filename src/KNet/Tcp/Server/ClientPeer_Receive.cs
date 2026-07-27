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
using System.Net.Sockets;

namespace KNet.Tcp.Server
{
    internal partial class ClientPeer
    {
        public void MultiThreadingReceiveSocketStream(SocketAsyncEventArgs e)
		{
			lock (mReceiveStreamList)
			{
                mReceiveStreamList.WriteFrom(e.MemoryBuffer.Span.Slice(e.Offset, e.BytesTransferred));
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
                    mServerMgr.mPackageManager.NetPackageExecute(mWrap, mNetPackage);
				}
			}

			return bSuccess;
		}
    }
}
