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
namespace KNet.LinuxTcp.Common
{
    internal class sk_buff_list
    {
        public sk_buff next;
        public sk_buff prev;
    }

    //这是一个双向链表，
    internal class sk_buff_head:sk_buff
    {
	    public uint qlen;
    }

    //这是一个双向链表，
    internal class list_head
    {
        public readonly sk_buff value;
        public list_head next;
        public list_head prev;

        public list_head(sk_buff t)
        {
            value = t;
            Reset();
        }

        public void Reset()
        {
            next = null;
            prev = null;
        }
    }

}
