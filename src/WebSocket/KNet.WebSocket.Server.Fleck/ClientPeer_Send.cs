// 客户端连接封装（分部类之二：发送相关）。
using KNet.Common;
using System;

namespace KNet.WebSocket.Server
{
    public partial class ClientPeer
    {
        public void SendNetData(ushort nPackageId) { }
        public void SendNetData(ushort nPackageId, byte[] data) { SendBinary(data); }
        public void SendNetData(ushort nPackageId, ReadOnlySpan<byte> buffer) { SendBinary(buffer.ToArray()); }
        public void SendNetData(NetPackage mNetPackage) { SendBinary(mNetPackage.GetData().ToArray()); }
        public void SendNetData(byte[] data) { SendBinary(data); }
        public void SendNetData(ReadOnlySpan<byte> data) { SendBinary(data.ToArray()); }

        private void SendBinary(byte[] data)
        {
            if (mSocketPeerState != SOCKET_PEER_STATE.CONNECTED) return;
            try { mSocket.Send(data); } catch { }
        }
    }
}
