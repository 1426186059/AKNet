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
using System.Net;

namespace KNet.LinuxTcp.Common
{
    internal interface UdpClientPeerCommonBase
    {
        void SetSocketState(SOCKET_PEER_STATE mState);
        SOCKET_PEER_STATE GetSocketState();
        void SendNetPackage(sk_buff skb);
        void SendInnerNetData(byte id);
        void NetPackageExecute(NetPackage mPackage);
        public void ResetSendHeartBeatCdTime();
        public void ReceiveHeartBeat();
        public void ReceiveConnect(sk_buff skb);
        public void ReceiveDisConnect();
        public ObjectPoolManager GetObjectPoolManager();
        IPEndPoint GetIPEndPoint();
    }
}
