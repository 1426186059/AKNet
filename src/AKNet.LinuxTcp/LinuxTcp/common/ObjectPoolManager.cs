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

namespace AKNet.LinuxTcp.Common
{
    internal class ObjectPoolManager
    {
        private readonly SafeObjectPool<sk_buff> mSkbPool = null;
        public ObjectPoolManager()
        {
            mSkbPool = new SafeObjectPool<sk_buff>(1024);
        }

        public sk_buff Skb_Pop()
        {
            return mSkbPool.Pop();
        }

        public void Skb_Recycle(sk_buff skb)
        {
            mSkbPool.recycle(skb);
        }

    }
}
