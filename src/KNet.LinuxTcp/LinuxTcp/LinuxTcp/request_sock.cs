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
    internal class request_sock : sock_common
    {
        public ushort mss;
        public byte num_retrans;
        public long ts_recent;
        public long timeout;
        public byte num_timeout;
    }

    internal class inet_request_sock : request_sock
    {
        public ushort snd_wscale;
        public ushort rcv_wscale;
        public ushort tstamp_ok;
        public ushort sack_ok;
        public ushort wscale_ok;
        public ushort ecn_ok;
        public ushort acked;
        public ushort no_srccheck;
        public ushort smc_ok;
    }

    internal class tcp_request_sock_ops
    {
        public ushort mss_clamp;
    }

    internal class tcp_request_sock : inet_request_sock
    {
        public tcp_request_sock_ops af_specific;
        public long snt_synack;
        public bool tfo_listener;
        public bool is_mptcp;
        public bool req_usec_ts;
        public bool drop_req;
        public uint txhash;
        public uint rcv_isn;
        public uint snt_isn;
        public uint ts_off;
        public long last_oow_ack_time;
        public uint rcv_nxt;
        public byte syn_tos;
        public byte ao_keyid;
        public byte ao_rcv_next;
        public bool used_tcp_ao;
    }

}
