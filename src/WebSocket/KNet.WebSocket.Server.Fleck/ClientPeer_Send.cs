// 客户端连接封装（分部类之二：发送相关，走 KNet 协议编码）。
using KNet.Common;
using System;

namespace KNet.WebSocket.Server
{
    public partial class ClientPeer
    {
        public void SendNetData(ushort nPackageId)
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
                SendNetStream(mCryptoMgr.Encode(nPackageId, ReadOnlySpan<byte>.Empty));
        }

        public void SendNetData(ushort nPackageId, byte[] data)
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
                SendNetStream(mCryptoMgr.Encode(nPackageId, data));
        }

        public void SendNetData(ushort nPackageId, ReadOnlySpan<byte> buffer)
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
                SendNetStream(mCryptoMgr.Encode(nPackageId, buffer));
        }

        public void SendNetData(NetPackage mNetPackage)
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
                SendNetStream(mCryptoMgr.Encode(mNetPackage.GetPackageId(), mNetPackage.GetData()));
        }

        public void SendNetData(byte[] data)
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
                SendNetStream(mCryptoMgr.Encode(0, data));
        }

        public void SendNetData(ReadOnlySpan<byte> data)
        {
            if (GetSocketState() == SOCKET_PEER_STATE.CONNECTED)
                SendNetStream(mCryptoMgr.Encode(0, data));
        }

        // 编码结果指向 CryptoMgr 内部缓冲，须立即拷贝为独立数组再发送
        private void SendNetStream(ReadOnlySpan<byte> mBufferSegment)
        {
            if (mSocketPeerState != SOCKET_PEER_STATE.CONNECTED) return;
            try { mSocket.Send(mBufferSegment.ToArray()); }
            catch { }
        }
    }
}
