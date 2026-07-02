/************************************Copyright*****************************************
 *  Project    : AKNet
 *  Web        : https://github.com/1426186059/AKNet
 *  Description: C# 游戏网络库
 *  Author     : 许珂
 *  Since      : 2024/11/01 00:00:00
 *  Updated    : 2026/07/02 18:05:45
 *  Copyright  : 作者保留一切版权权利, 商业用途需支付版权费用
 *  Contact    : 微信：AAA-2025-666-888
************************************Copyright*****************************************/
using AKNet.Common;

namespace AKNet.Udp3Tcp.Common
{
    internal class ObjectPoolManager
    {
        private readonly SafeObjectPool<NetUdpSendFixedSizePackage> mSendPackagePool = null;
        private readonly SafeObjectPool<NetUdpReceiveFixedSizePackage> mReceivePackagePool = null;

        public ObjectPoolManager()
        {
            mSendPackagePool = new SafeObjectPool<NetUdpSendFixedSizePackage>(1024);
            mReceivePackagePool = new SafeObjectPool<NetUdpReceiveFixedSizePackage>(1024);
        }

        public NetUdpSendFixedSizePackage UdpSendPackage_Pop()
        {
            return mSendPackagePool.Pop();
        }

        public void UdpSendPackage_Recycle(NetUdpSendFixedSizePackage mPackage)
        {
            mSendPackagePool.recycle(mPackage);
        }

        public NetUdpReceiveFixedSizePackage UdpReceivePackage_Pop()
        {
            return mReceivePackagePool.Pop();
        }

        public void UdpReceivePackage_Recycle(NetUdpReceiveFixedSizePackage mPackage)
        {
            mReceivePackagePool.recycle(mPackage);
        }

    }
}
