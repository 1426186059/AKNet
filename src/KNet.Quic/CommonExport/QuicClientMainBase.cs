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
using System.Net;
namespace KNet.Common
{
    public class QuicClientMainBase : QuicClientInterface, QuicClientPeerBase
    {

        protected QuicClientInterface mInterface = null;
        public QuicClientInterface GetInstance()
        {
            return mInterface;
        }

        public void SetInstance(QuicClientInterface mInterface)
        {
            this.mInterface = mInterface;
        }

        public void ConnectServer(string Ip, int nPort)
        {
            mInterface.ConnectServer(Ip, nPort);
        }

        public bool DisConnectServer()
        {
            return mInterface.DisConnectServer();
        }

        public IPEndPoint GetIPEndPoint()
        {
            return mInterface.GetIPEndPoint();
        }

        public SOCKET_PEER_STATE GetSocketState()
        {
            return mInterface.GetSocketState();
        }

        public void ReConnectServer()
        {
            mInterface.ReConnectServer();
        }

        public void Dispose()
        {
            mInterface.Dispose();
        }

        public void addListenClientPeerStateFunc(Action<QuicClientPeerBase, SOCKET_PEER_STATE> mFunc)
        {
            mInterface.addListenClientPeerStateFunc(mFunc);
        }

        public void addListenClientPeerStateFunc(Action<QuicClientPeerBase> mFunc)
        {
            mInterface.addListenClientPeerStateFunc(mFunc);
        }

        public void removeListenClientPeerStateFunc(Action<QuicClientPeerBase, SOCKET_PEER_STATE> mFunc)
        {
            mInterface.removeListenClientPeerStateFunc(mFunc);
        }

        public void removeListenClientPeerStateFunc(Action<QuicClientPeerBase> mFunc)
        {
            mInterface.removeListenClientPeerStateFunc(mFunc);
        }

        public void addNetListenFunc(ushort nPackageId, Action<QuicClientPeerBase, QuicNetPackage> fun)
        {
            mInterface.addNetListenFunc(nPackageId, fun);
        }

        public void removeNetListenFunc(ushort nPackageId, Action<QuicClientPeerBase, QuicNetPackage> fun)
        {
            mInterface.removeNetListenFunc(nPackageId, fun);
        }

        public void addNetListenFunc(Action<QuicClientPeerBase, QuicNetPackage> func)
        {
            mInterface.addNetListenFunc(func);
        }

        public void removeNetListenFunc(Action<QuicClientPeerBase, QuicNetPackage> func)
        {
            mInterface.removeNetListenFunc(func);
        }

        public void SendNetData(byte nStreamIndex, ushort nPackageId)
        {
            mInterface.SendNetData(nStreamIndex, nPackageId);
        }

        public void SendNetData(byte nStreamIndex, ushort nPackageId, byte[] data)
        {
            mInterface.SendNetData(nStreamIndex, nPackageId, data);
        }

        public void SendNetData(byte nStreamIndex, NetPackage mNetPackage)
        {
            mInterface.SendNetData(nStreamIndex, mNetPackage);
        }

        public void SendNetData(byte nStreamIndex, ushort nPackageId, ReadOnlySpan<byte> buffer)
        {
            mInterface.SendNetData(nStreamIndex, nPackageId, buffer);
        }

        public void Update(double elapsed)
        {
            mInterface.Update(elapsed);
        }

        public void Update() { mInterface.Update(); }

        public void SetName(string name)
        {
            mInterface.SetName(name);
        }

        public string GetName()
        {
            return mInterface.GetName();
        }

        public void SetID(uint id)
        {
            mInterface.SetID(id);
        }

        public uint GetID()
        {
            return mInterface.GetID();
        }

        public void SetOwner(object owner) { mInterface.SetOwner(owner); }
        public object GetOwner() { return mInterface.GetOwner(); }

        public void SendNetData(byte nStreamIndex, byte[] data) { mInterface.SendNetData(nStreamIndex, 0, data); }
        public void SendNetData(byte nStreamIndex, ReadOnlySpan<byte> data) { mInterface.SendNetData(nStreamIndex, 0, data); }
    }
}
