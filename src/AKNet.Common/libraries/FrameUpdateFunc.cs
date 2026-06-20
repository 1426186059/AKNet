/************************************Copyright*****************************************
*        ProjectName:AKNet
*        Web:https://github.com/825126369/AKNet
*        Description:AKNet 网络库, 兼容 C#8.0 和 .Net Standard 2.1
*        Author:阿珂
*        CreateTime:2024/10/30 21:55:40
*        Copyright:MIT软件许可证
************************************Copyright*****************************************/
using System;
using System.Diagnostics;

namespace AKNet.Common
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
