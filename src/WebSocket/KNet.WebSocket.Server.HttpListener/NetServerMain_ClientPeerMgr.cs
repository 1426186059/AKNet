// ClientPeer 管理器：活跃连接登记/移除、心跳超时检测、状态变更派发。
using KNet.Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace KNet.WebSocket.Server
{
    public partial class NetServerMain : NetServerInterface
    {
        // 由定时器驱动：发送心跳 + 检测接收心跳超时（超时者从列表移除并关闭）
        // 优化（对齐 TcpListener 思路）：锁只用于“跨线程交接 + 列表结构变更”，绝不包住逐连接的工作。
        // TcpListener 把跨线程交接收敛到 mConnectSocketQueue（仅它加锁），客户端列表只在主线程改动故无需锁遍历；
        // HttpListener 的 AcceptLoop/RecvLoop 在各自线程上直接改动列表，列表确为多线程写入，但仍应把锁收敛到
        // 最短范围——遍历与 peer.Update 在锁外执行，避免阻塞 RecvLoopAsync finally 的移除/通知。
        public void Update(double elapsed)
        {
            if (elapsed >= 0.3) NetLog.LogWarning("帧 时间 太长: " + elapsed);

            // 仅用短锁拷贝一份快照；随后遍历快照、peer.Update、peer.Dispose 均在锁外执行
            ClientPeer[] snapshot;
            lock (mClientListLock) { snapshot = mClientList.ToArray(); }

            List<ClientPeer> toRemove = null;
            foreach (var peer in snapshot)
            {
                peer.Update(elapsed); // 锁外：不阻塞其它线程的增删
                if (peer.GetSocketState() != SOCKET_PEER_STATE.CONNECTED)
                {
                    (toRemove ??= new List<ClientPeer>()).Add(peer);
                }
            }

            if (toRemove != null)
            {
                // 仅“真正把 peer 移出列表”的一方负责释放与通知（Remove 返回 true 时），保证 Dispose/通知恰好一次
                // 移除本身在短锁内完成；Dispose 在锁外，避免 RecvLoopAsync finally 再取锁时死锁
                List<ClientPeer> removed = null;
                lock (mClientListLock)
                {
                    foreach (var peer in toRemove)
                    {
                        if (mClientList.Remove(peer)) (removed ??= new List<ClientPeer>()).Add(peer);
                    }
                }
                if (removed != null)
                {
                    foreach (var peer in removed)
                    {
                        try { peer.Dispose(); } catch { }
                        NotifyDisconnected(peer);
                    }
                }
            }
        }

        public void OnClientConnected(ClientPeer peer)
        {
            peer.SetSocketState(SOCKET_PEER_STATE.CONNECTED);
            lock (mClientListLock) { mClientList.Add(peer); }
            OnSocketStateChanged(peer);
            NetLog.Log($"[HttpListener] 客户端连接: {peer.GetIPEndPoint()}  当前在线: {GetClientCount()}");
        }

        public void OnClientDisconnected(ClientPeer peer)
        {
            bool bRemoved;
            lock (mClientListLock) { bRemoved = mClientList.Remove(peer); }
            peer.SetSocketState(SOCKET_PEER_STATE.DISCONNECTED);
            if (bRemoved)
            {
                // 由真正将其移出列表的一方负责释放与通知（与 Update 超时移除路径一致），避免重复
                try { peer.Dispose(); } catch { }
                NotifyDisconnected(peer);
            }
        }

        private void NotifyDisconnected(ClientPeer peer)
        {
            OnSocketStateChanged(peer);
            NetLog.Log($"[HttpListener] 客户端断开: {peer.GetIPEndPoint()}  当前在线: {GetClientCount()}");
        }
    }
}
