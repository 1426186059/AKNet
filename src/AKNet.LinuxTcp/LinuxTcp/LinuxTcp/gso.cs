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
namespace AKNet.LinuxTcp.Common
{
    internal static partial class LinuxTcpFunc
    {
        //static int skb_gso_transport_seglen(sk_buff skb)
        //{
        //    skb_shared_info shinfo = skb_shinfo(skb);
        //    int thlen = 0;

        //    if (BoolOk(shinfo.gso_type & (SKB_GSO_TCPV4 | SKB_GSO_TCPV6)))
        //    {
        //        thlen = tcp_hdrlen(skb);
        //    }

        //    return thlen + shinfo.gso_size;
        //}

        //static int skb_gso_network_seglen(sk_buff skb)
        //{
        //    int hdr_len = skb.transport_header - skb.network_header;
        //    return hdr_len + skb_gso_transport_seglen(skb);
        //}

        //static bool skb_gso_size_check(sk_buff skb, int seg_len, int max_len)
        //{
        //    skb_shared_info shinfo = skb_shinfo(skb);
        //    if (shinfo.gso_size != GSO_BY_FRAGS)
        //    {
        //        return seg_len <= max_len;
        //    }

        //    seg_len -= GSO_BY_FRAGS;
        //    for (sk_buff iter = skb_shinfo(skb).frag_list; iter != null; iter = iter.next)
        //    {
        //        if (seg_len + skb_headlen(iter) > max_len)
        //        {
        //            return false;
        //        }
        //    }
        //    return true;

        //}

        //static bool skb_gso_validate_network_len(sk_buff skb, int mtu)
        //{
        //    return skb_gso_size_check(skb, skb_gso_network_seglen(skb), mtu);
        //}
    }
}
