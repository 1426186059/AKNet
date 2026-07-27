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
using System;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("KNet")]
[assembly: InternalsVisibleTo("KNet.MSQuic")]
[assembly: InternalsVisibleTo("KNet.LinuxTcp")]
[assembly: InternalsVisibleTo("KNet.WebSocket")]
namespace KNet.Common
{
    internal static class TimeTool
    {
        /// <summary>
        /// 时间戳转为C#格式时间
        /// </summary>
        /// <param name=”timeStamp”></param>
        /// <returns></returns>
        public static DateTime GetDateTime(ulong timeStamp)
        {
            DateTime dtStart = TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1));
            return dtStart.AddSeconds(timeStamp);
        }

        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name=”time”></param>
        /// <returns></returns>
        public static long GetTimeStamp(System.DateTime time)
        {
            DateTime startTime = TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1));
            return (long)(time - startTime).TotalSeconds;
        }

        public static long GetTimeStamp()
        {
            System.DateTime time = DateTime.Now;
            DateTime startTime = TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1));
            return (long)(time - startTime).TotalSeconds;
        }
    }
}
