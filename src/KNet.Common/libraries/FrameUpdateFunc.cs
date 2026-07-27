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
using System.Diagnostics;

namespace KNet.Common
{
    internal class FrameUpdateFunc
    {
        private readonly Stopwatch mStopWatch = Stopwatch.StartNew();
        long nLastTime = 0;
        public void Update(Action<double> updateFunc)
        {
            if (nLastTime == 0)
            {
                nLastTime = mStopWatch.ElapsedMilliseconds;
            }

            double deltaTime = (mStopWatch.ElapsedMilliseconds - nLastTime) / 1000.0;
            nLastTime = mStopWatch.ElapsedMilliseconds;
            updateFunc(deltaTime);
        }
    }
}
