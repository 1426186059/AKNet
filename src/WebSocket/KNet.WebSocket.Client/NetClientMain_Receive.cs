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
using System.Net.WebSockets;

namespace KNet.WebSocket.Client
{
    public partial class NetClientMain
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
