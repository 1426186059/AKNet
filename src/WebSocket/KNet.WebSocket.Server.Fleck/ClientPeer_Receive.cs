// 客户端连接封装（分部类之三：接收与解码）。
using KNet.Common;
using System;

namespace KNet.WebSocket.Server
{
    public partial class ClientPeer
    {
        // 收到一帧 WebSocket 二进制消息：写入接收流后循环解出完整 KNet 包
        public void OnBinaryReceived(byte[] data)
        {
            lock (mReceiveStreamList) { mReceiveStreamList.WriteFrom(data.AsSpan()); }
            NetPackageExecute();
        }

        private void NetPackageExecute()
        {
            while (true)
            {
                bool bSuccess;
                lock (mReceiveStreamList) { bSuccess = mCryptoMgr.Decode(mReceiveStreamList, mNetPackage); }
                if (!bSuccess) break;

                if (CommonTcpLayerNetCommand.orInnerCommand(mNetPackage.nPackageId))
                {
                    // 心跳等内部命令：回包保活，不派发给业务层
                    SendNetData(CommonTcpLayerNetCommand.COMMAND_HEARTBEAT);
                }
                else
                {
                    mServerMgr.Dispatch(this, mNetPackage);
                }
            }
        }
    }
}
