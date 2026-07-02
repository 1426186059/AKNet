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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AKNet.Platform.Socket
{
    internal sealed class SafeNativeOverlapped : SafeHandle
    {
        private readonly IntPtr _socketHandle;

        public SafeNativeOverlapped()
            : this(IntPtr.Zero)
        {
            
        }

        private SafeNativeOverlapped(IntPtr handle)
            : base(IntPtr.Zero, true)
        {
            SetHandle(handle);
        }

        public unsafe SafeNativeOverlapped(IntPtr socketHandle, NativeOverlapped* handle)
            : this((IntPtr)handle)
        {
            _socketHandle = socketHandle;
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            FreeNativeOverlapped();
            return true;
        }

        private unsafe void FreeNativeOverlapped()
        {
           
        }
    }
}
