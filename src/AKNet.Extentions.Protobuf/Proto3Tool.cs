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
using AKNet.Common;
using Google.Protobuf;
using System;

namespace AKNet.Extentions.Protobuf
{
    public static class Proto3Tool
    {
        public static ReadOnlySpan<byte> SerializePackage(IMessage data)
        {
            return SerializePackage(data, EnSureSendBufferOk(data));
        }

        public static ReadOnlySpan<byte> SerializePackage(IMessage data, byte[] cacheSendBuffer)
		{
            int Length = data.CalculateSize();
            Span<byte> output = new Span<byte>(cacheSendBuffer, 0, Length);
			data.WriteTo(output);
			return output;
		}
		
		public static T GetData<T>(ReadOnlySpan<byte> mReadOnlySpan) where T : class, IMessage, IMessage<T>, new()
		{
            T t = MessageParserEx<T>.Parser.ParseFrom(mReadOnlySpan);
            return t;
        }

        public static T GetData<T>(NetPackage mPackage) where T : class, IMessage, IMessage<T>, new()
        {
            T t = MessageParserEx<T>.Parser.ParseFrom(mPackage.GetData());
            return t;
        }

        public static T GetPoolData<T>(ReadOnlySpan<byte> mReadOnlySpan) where T : class, IMessage, IMessage<T>, IProtobufResetInterface, new()
        {
            T t = MessageParserPool<T>.Parser.ParseFrom(mReadOnlySpan);
            return t;
        }

        public static T GetPoolData<T>(NetPackage mPackage) where T : class, IMessage, IMessage<T>, IProtobufResetInterface, new()
		{
            T t = MessageParserPool<T>.Parser.ParseFrom(mPackage.GetData());
            return t;
        }

        private static byte[] cacheSendProtobufBuffer = new byte[1024];
        private static byte[] EnSureSendBufferOk(IMessage data)
        {
            int Length = data.CalculateSize();
            BufferTool.EnSureBufferOk_Power2(ref cacheSendProtobufBuffer, Length);
            return cacheSendProtobufBuffer;
        }
    }
}