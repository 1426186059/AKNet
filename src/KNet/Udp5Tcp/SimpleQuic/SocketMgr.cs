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
using KNet.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace KNet.Udp5Tcp.Common
{
    internal partial class SocketMgr : IDisposable
    {
        public class Config
        {
            public bool bServer;
            public EndPoint mEndPoint;
            public Action<SocketAsyncEventArgs> mReceiveFunc;
        }

        readonly List<SocketItem> mSocketList = new List<SocketItem>();
        public E_LOGIC_RESULT InitNet(Config mConfig)
		{
            try
            {
                int nSocketCount = mConfig.bServer ? KNet.Udp5Tcp.Common.Config.nSocketCount : 1;
                for (int i = 0; i < nSocketCount; i++)
                {
                    var mSocketItem = new SocketItem(mConfig);
                    mSocketList.Add(mSocketItem);
                    mSocketItem.InitNet();
                }
            }
            catch (Exception ex)
            {
                NetLog.LogError($"SocketMgr 初始化失败: {ex.Message}");
                return E_LOGIC_RESULT.Error;
            }

            return E_LOGIC_RESULT.Success;
        }

        public SocketItem GetSocketItem(int nSocketIndex)
        {
            return mSocketList[nSocketIndex];
        }

        public void Dispose()
        {
            for (int i = 0; i < mSocketList.Count; i++)
            {
                mSocketList[i].Dispose();
            }
            mSocketList.Clear();
        }
    }

}









