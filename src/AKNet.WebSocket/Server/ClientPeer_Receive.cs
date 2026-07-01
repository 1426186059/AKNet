/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库
*        Author:许珂
*        StartTime:2024/11/01 00:00:00
*        ModifyTime:2026/2/1 20:26:47
*        Copyright:MIT软件许可证
************************************Copyright*****************************************/
using AKNet.Common;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace AKNet.WebSocket.Server
{
    using WsWebSocket = System.Net.WebSockets.WebSocket;

    internal partial class ClientPeer
    {
        private async Task ReceiveLoopAsync()
        {
            var receiveBuffer = new byte[1024 * 64];

            try
            {
                while (true)
                {
                    WsWebSocket ws;
                    lock (mWsLock) { ws = mWebSocket; }
                    if (ws == null || ws.State != WebSocketState.Open) break;

                    var result = await ws.ReceiveAsync(
                        new ArraySegment<byte>(receiveBuffer), CancellationToken.None)
                        .ConfigureAwait(false);

                    if (result.MessageType == WebSocketMessageType.Close) break;

                    if (result.Count > 0)
                    {
                        lock (mReceiveStreamList)
                        {
                            mReceiveStreamList.WriteFrom(
                                new ReadOnlySpan<byte>(receiveBuffer, 0, result.Count));
                        }
                    }
                }
            }
            catch { }
            finally
            {
                DisConnectedWithNormal();
            }
        }

        private bool NetPackageExecute()
        {
            NetStreamReceivePackage mNetPackage = mServerMgr.mNetPackage;
            bool bSuccess = false;
            lock (mReceiveStreamList) { bSuccess = mServerMgr.mCryptoMgr.Decode(mReceiveStreamList, mNetPackage); }
            if (bSuccess && !CommonTcpLayerNetCommand.orInnerCommand(mNetPackage.nPackageId))
                mServerMgr.mPackageManager.NetPackageExecute(mWrap, mNetPackage);
            return bSuccess;
        }
    }
}
