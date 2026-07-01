/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:C#游戏网络库
*        Author:许珂
*        StartTime:2024/11/01 00:00:00
*        ModifyTime:2026/2/1 20:27:06
*        Copyright:MIT软件许可证
************************************Copyright*****************************************/
using AKNet.Common;
using AKNet.LinuxTcp.Common;
using System;

namespace AKNet.LinuxTcp.Client
{
    internal partial class NetClientMain
    {
        private void SendHeartBeat()
        {
            SendInnerNetData(UdpNetCommand.COMMAND_HEARTBEAT);
        }

        public void ResetSendHeartBeatCdTime()
        {
            fMySendHeartBeatCdTime = 0.0;
        }

        public void ReceiveHeartBeat()
        {
            fReceiveHeartBeatTime = 0.0;
        }

        public void SendConnect()
        {
            fConnectCdTime = 0.0;
            fDisConnectCdTime = 0.0;
            fReConnectServerCdTime = 0.0;
            fReceiveHeartBeatTime = 0.0;
            fMySendHeartBeatCdTime = 0.0;

            this.Reset();
            mUdpCheckPool.InitConnect();
            SetSocketState(SOCKET_PEER_STATE.CONNECTING);
            NetLog.Log("Client: Udp 正在连接服务器: " + remoteEndPoint);
            SendInnerNetData(UdpNetCommand.COMMAND_CONNECT);
        }

        public void SendDisConnect()
        {
            fConnectCdTime = 0.0;
            fDisConnectCdTime = 0.0;
            fReConnectServerCdTime = 0.0;
            fReceiveHeartBeatTime = 0.0;
            fMySendHeartBeatCdTime = 0.0;

            this.Reset();
            SetSocketState(SOCKET_PEER_STATE.DISCONNECTING);
            NetLog.Log("Client: Udp 正在 断开服务器: " + remoteEndPoint);
            SendInnerNetData(UdpNetCommand.COMMAND_DISCONNECT);
        }

        public void ReceiveConnect(sk_buff skb)
        {
            if (GetSocketState() != SOCKET_PEER_STATE.CONNECTED)
            {
                fConnectCdTime = 0.0;
                fDisConnectCdTime = 0.0;
                fReConnectServerCdTime = 0.0;
                fReceiveHeartBeatTime = 0.0;
                fMySendHeartBeatCdTime = 0.0;

                this.Reset();
                mUdpCheckPool.FinishConnect(skb);
                SetSocketState(SOCKET_PEER_STATE.CONNECTED);
                NetLog.Log("Client: Udp连接服务器 成功 ! ");
            }
        }

        public void ReceiveDisConnect()
        {
            if (GetSocketState() != SOCKET_PEER_STATE.DISCONNECTED)
            {
                fConnectCdTime = 0.0;
                fDisConnectCdTime = 0.0;
                fReConnectServerCdTime = 0.0;
                fReceiveHeartBeatTime = 0.0;
                fMySendHeartBeatCdTime = 0.0;

                this.Reset();
                SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
                NetLog.Log("Client: Udp 断开服务器 成功 ! ");
            }
        }
    }
}
