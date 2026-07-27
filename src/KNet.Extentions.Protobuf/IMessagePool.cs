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
using Google.Protobuf;
using System;
using System.Collections.Generic;

namespace KNet.Extentions.Protobuf
{
    internal static class MessageParserEx<T> where T : class, IMessage, IMessage<T>, new()
    {
        public static readonly MessageParser<T> Parser = new MessageParser<T>(factory);
        private static T factory()
        {
            return new T();
        }
    }

    internal static class MessageParserPool<T> where T : class, IMessage, IMessage<T>, IProtobufResetInterface, new()
	{
		public static readonly MessageParser<T> Parser = new MessageParser<T>(factory);
        private static T factory()
        {
            return IMessagePool<T>.Pop();
        }
	}

    public interface IProtobufResetInterface
    {
        void Reset();
    }

	public static class IMessagePool<T> where T : class, IMessage, IMessage<T>, IProtobufResetInterface, new()
	{
		readonly static Stack<T> mObjectPool = new Stack<T>();
		private static int nMaxCapacity = 0;

        static IMessagePool()
        {
            nMaxCapacity = 1;
        }

        public static void SetMaxCapacity(int nCapacity)
        {
            nMaxCapacity = nCapacity;
        }

        public static int Count()
		{
			return mObjectPool.Count;
		}

		public static T Pop()
		{
			T t = null;
			if (!mObjectPool.TryPop(out t))
			{
				t = new T();
			}
			return t;
		}

#if DEBUG
		//Protobuf内部实现了相等器,所以不能直接通过 == 来比较是否包含 
		private static bool orContain(T t)
		{
			foreach (var v in mObjectPool)
			{
				if (Object.ReferenceEquals(v, t))
				{
					return true;
				}
			}
			return false;
		}
#endif

		public static void recycle(T t)
		{
#if DEBUG
            NetLog.Assert(!orContain(t));
#endif

            t.Reset();
            //防止 内存一直增加，合理的GC
            bool bRecycle = nMaxCapacity <= 0 || mObjectPool.Count < nMaxCapacity;
			if (bRecycle)
			{
				mObjectPool.Push(t);
			}
		}
	}
}
