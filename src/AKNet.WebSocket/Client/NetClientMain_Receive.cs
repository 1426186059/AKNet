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
using System.Threading.Tasks;

namespace AKNet.WebSocket.Client
{
    internal partial class NetClientMain
    {
        private async Task ReceiveLoopAsync()
        {
            var receiveBuffer = new byte[1024 * 64];

            try
            {
                while (true)
                {
                    ClientWebSocket ws;
                    lock (mWsLock) { ws = mWebSocket; }
                    if (ws == null || ws.State != WebSocketState.Open) break;

                    var result = await ws.ReceiveAsync(
                        new ArraySegment<byte>(receiveBuffer), System.Threading.CancellationToken.None)
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
                MainThreadCheck.Check();
                bReceiveTaskRunning = false;
                DisConnectedWithError();
            }
        }

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
