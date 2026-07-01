/************************************Copyright*****************************************
 *  Project    : AKNet
 *  Web        : https://github.com/825126369/AKNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/07/02 04:26:51
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using AKNet.Common;
using Google.Protobuf;
using System;

namespace AKNet.Extentions.Protobuf
{
    public static class ClientPeerBaseExtentions
    {
        public static void SendNetData(this ClientPeerBase mInterface, ushort nPackageId, IMessage data)
        {
            if (mInterface.GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
            {
                ReadOnlySpan<byte> stream = Proto3Tool.SerializePackage(data);
                mInterface.SendNetData(nPackageId, stream);
            }
        }
    }
}
