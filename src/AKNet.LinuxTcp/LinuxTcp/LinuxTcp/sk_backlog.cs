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
using System.Collections.Generic;

namespace AKNet.LinuxTcp.Common
{
    internal class sk_backlog
    {
        public LinkedList<sk_buff> mQueue = new LinkedList<sk_buff>();
        public long rmem_alloc;
        public int len;

        public sk_buff head
        {
            get 
            {
                if (mQueue.First != null)
                {
                    return mQueue.First.Value;
                }
                return null;
            }
        }

        public sk_buff tail
        {
            get 
            {
                if (mQueue.Last != null)
                {
                    return mQueue.Last.Value;
                }

                return null;
            }
        }
    }
}
