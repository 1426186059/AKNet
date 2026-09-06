// KNet NetPackage 的轻量实现（仅承载原始字节，供事件分发使用）。
using KNet.Common;
using System;

namespace KNet.WebSocket.Server
{
    internal class NetPackageImpl : NetPackage
    {
        private readonly ushort mId;
        private readonly byte[] mData;
        public NetPackageImpl(ushort id, byte[] data) { mId = id; mData = data; }
        public ushort GetPackageId() => mId;
        public ReadOnlySpan<byte> GetData() => mData;
    }
}
