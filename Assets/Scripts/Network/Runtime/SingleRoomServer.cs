using System;
using System.Collections.Generic;

namespace FurryChaos.Networking
{
    /// <summary>单房间连接和消息路由，不包含具体角色生成或 KCC 模拟。</summary>
    public sealed class SingleRoomServer : IDisposable
    {
        private readonly INetworkTransport m_Transport;
        private readonly Dictionary<int, long> m_EntitiesByConnection = new();
        private long m_NextEntityId = 1;

        public event Action<long> PlayerJoined;
        public event Action<long> PlayerLeft;
        public event Action<int, NetworkInputFrame> InputReceived;

        public SingleRoomServer(INetworkTransport transport)
        {
            m_Transport = transport;
            m_Transport.Connected += OnConnected;
            m_Transport.Disconnected += OnDisconnected;
            m_Transport.DataReceived += OnDataReceived;
        }

        public void BroadcastSnapshot(in NetworkCharacterSnapshot snapshot)
        {
            m_Transport.Broadcast(NetworkPacketCodec.EncodeSnapshot(snapshot));
        }

        public void Dispose()
        {
            m_Transport.Connected -= OnConnected;
            m_Transport.Disconnected -= OnDisconnected;
            m_Transport.DataReceived -= OnDataReceived;
            m_EntitiesByConnection.Clear();
        }

        private void OnConnected(int connectionId)
        {
            // 等待 JoinRequest 后再分配 EntityId，避免未完成协议握手的连接进入房间。
        }

        private void OnDisconnected(int connectionId)
        {
            if (!m_EntitiesByConnection.TryGetValue(connectionId, out var entityId))
                return;

            m_EntitiesByConnection.Remove(connectionId);
            PlayerLeft?.Invoke(entityId);
        }

        private void OnDataReceived(int connectionId, byte[] payload)
        {
            if (!NetworkPacketCodec.TryGetMessageType(payload, out var messageType))
                return;

            switch (messageType)
            {
                case NetworkMessageType.JoinRequest:
                    HandleJoinRequest(connectionId, payload);
                    break;

                case NetworkMessageType.InputFrame:
                    HandleInputFrame(connectionId, payload);
                    break;
            }
        }

        private void HandleJoinRequest(int connectionId, byte[] payload)
        {
            if (!NetworkPacketCodec.TryDecodeJoinRequest(payload, out var protocolVersion) ||
                protocolVersion != NetworkPacketCodec.ProtocolVersion)
            {
                return;
            }

            if (!m_EntitiesByConnection.TryGetValue(connectionId, out var entityId))
            {
                entityId = m_NextEntityId++;
                m_EntitiesByConnection.Add(connectionId, entityId);
                PlayerJoined?.Invoke(entityId);
            }

            m_Transport.Send(connectionId, NetworkPacketCodec.EncodeJoinAccepted(entityId));
        }

        private void HandleInputFrame(int connectionId, byte[] payload)
        {
            if (!m_EntitiesByConnection.TryGetValue(connectionId, out var entityId) ||
                !NetworkPacketCodec.TryDecodeInputFrame(payload, out var input))
            {
                return;
            }

            // 永远使用服务端会话映射的 EntityId，不能信任客户端上传的身份。
            input.EntityId = entityId;
            InputReceived?.Invoke(connectionId, input);
        }
    }
}
