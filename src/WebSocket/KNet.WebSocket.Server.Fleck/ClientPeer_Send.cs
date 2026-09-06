// 客户端连接封装（分部类之二：发送相关，走 KNet 协议编码）。
using KNet.Common;
using System;

namespace KNet.WebSocket.Server
{
    public partial class ClientPeer
    {
        public void SendNetData(ushort nPackageId) { EncodeAndSend(nPackageId, ReadOnlySpan<byte>.Empty); }
        public void SendNetData(ushort nPackageId, byte[] data) { EncodeAndSend(nPackageId, data); }
        public void SendNetData(ushort nPackageId, ReadOnlySpan<byte> buffer) { EncodeAndSend(nPackageId, buffer); }
        public void SendNetData(NetPackage mNetPackage) { EncodeAndSend(mNetPackage.GetPackageId(), mNetPackage.GetData()); }
        public void SendNetData(byte[] data) { EncodeAndSend(0, data); }
        public void SendNetData(ReadOnlySpan<byte> data) { EncodeAndSend(0, data); }

        // 编码结果指向 CryptoMgr 内部缓冲，须在锁内拷贝为独立数组；
        // 锁避免定时器心跳发送与接收回显在同一连接上并发改写同一编码缓冲
        private void EncodeAndSend(ushort nPackageId, ReadOnlySpan<byte> body)
        {
            if (GetSocketState() != SOCKET_PEER_STATE.CONNECTED) return;
            byte[] buf;
            lock (mCryptoLock) { buf = mCryptoMgr.Encode(nPackageId, body).ToArray(); }
            try { mSocket.Send(buf); }
            catch { }
        }
    }
}
