using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;

namespace FurryChaos.Networking.Transports
{
    public sealed class UnityTransportAdapter : INetworkTransport
    {
        private struct ServerConnection
        {
            public int Id;
            public NetworkConnection Connection;
        }

        private readonly List<ServerConnection> m_ServerConnections = new();
        private NetworkDriver m_Driver;
        private NetworkConnection m_ClientConnection;
        private int m_NextConnectionId = 1;

        public bool IsRunning { get; private set; }
        public bool IsServer { get; private set; }

        public event Action<int> Connected;
        public event Action<int> Disconnected;
        public event Action<int, byte[]> DataReceived;

        public bool StartServer(ushort port)
        {
            Stop();

            m_Driver = NetworkDriver.Create();
            var endpoint = NetworkEndpoint.AnyIpv4.WithPort(port);
            if (m_Driver.Bind(endpoint) != 0)
            {
                m_Driver.Dispose();
                return false;
            }

            m_Driver.Listen();
            IsServer = true;
            IsRunning = true;
            return true;
        }

        public bool StartClient(string address, ushort port)
        {
            Stop();

            try
            {
                m_Driver = NetworkDriver.Create();
                var endpoint = NetworkEndpoint.Parse(address, port);
                m_ClientConnection = m_Driver.Connect(endpoint);
                IsServer = false;
                IsRunning = m_ClientConnection.IsCreated;
                return IsRunning;
            }
            catch
            {
                if (m_Driver.IsCreated)
                    m_Driver.Dispose();

                return false;
            }
        }

        public void Poll()
        {
            if (!IsRunning || !m_Driver.IsCreated)
                return;

            m_Driver.ScheduleUpdate().Complete();

            if (IsServer)
                PollServer();
            else
                PollClient();
        }

        public void Send(int connectionId, byte[] payload)
        {
            if (!IsRunning || payload == null || payload.Length == 0)
                return;

            if (IsServer)
            {
                for (var i = 0; i < m_ServerConnections.Count; i++)
                {
                    if (m_ServerConnections[i].Id == connectionId)
                    {
                        Send(m_ServerConnections[i].Connection, payload);
                        return;
                    }
                }
            }
            else if (connectionId == 0 && m_ClientConnection.IsCreated)
            {
                Send(m_ClientConnection, payload);
            }
        }

        public void Broadcast(byte[] payload)
        {
            if (!IsServer || payload == null || payload.Length == 0)
                return;

            for (var i = 0; i < m_ServerConnections.Count; i++)
                Send(m_ServerConnections[i].Connection, payload);
        }

        public void Stop()
        {
            if (m_Driver.IsCreated)
            {
                if (IsServer)
                {
                    for (var i = 0; i < m_ServerConnections.Count; i++)
                    {
                        if (m_ServerConnections[i].Connection.IsCreated)
                            m_Driver.Disconnect(m_ServerConnections[i].Connection);
                    }
                }
                else if (m_ClientConnection.IsCreated)
                {
                    m_Driver.Disconnect(m_ClientConnection);
                }

                m_Driver.Dispose();
            }

            m_ServerConnections.Clear();
            m_ClientConnection = default;
            m_NextConnectionId = 1;
            IsRunning = false;
            IsServer = false;
        }

        public void Dispose()
        {
            Stop();
        }

        private void PollServer()
        {
            NetworkConnection accepted;
            while ((accepted = m_Driver.Accept()) != default)
            {
                var connection = new ServerConnection
                {
                    Id = m_NextConnectionId++,
                    Connection = accepted
                };
                m_ServerConnections.Add(connection);
                Connected?.Invoke(connection.Id);
            }

            for (var i = m_ServerConnections.Count - 1; i >= 0; i--)
            {
                var item = m_ServerConnections[i];
                NetworkEvent.Type eventType;
                while ((eventType = m_Driver.PopEventForConnection(
                           item.Connection, out var reader)) != NetworkEvent.Type.Empty)
                {
                    if (eventType == NetworkEvent.Type.Data)
                    {
                        DataReceived?.Invoke(item.Id, ReadPayload(reader));
                    }
                    else if (eventType == NetworkEvent.Type.Disconnect)
                    {
                        Disconnected?.Invoke(item.Id);
                        m_ServerConnections.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private void PollClient()
        {
            NetworkEvent.Type eventType;
            while ((eventType = m_ClientConnection.PopEvent(
                       m_Driver, out var reader)) != NetworkEvent.Type.Empty)
            {
                if (eventType == NetworkEvent.Type.Connect)
                {
                    Connected?.Invoke(0);
                }
                else if (eventType == NetworkEvent.Type.Data)
                {
                    DataReceived?.Invoke(0, ReadPayload(reader));
                }
                else if (eventType == NetworkEvent.Type.Disconnect)
                {
                    Disconnected?.Invoke(0);
                    m_ClientConnection = default;
                    IsRunning = false;
                }
            }
        }

        private void Send(NetworkConnection connection, byte[] payload)
        {
            if (m_Driver.BeginSend(connection, out var writer) != 0)
                return;

            if (payload.Length > writer.Capacity)
            {
                m_Driver.AbortSend(writer);
                return;
            }

            for (var i = 0; i < payload.Length; i++)
                writer.WriteByte(payload[i]);

            m_Driver.EndSend(writer);
        }

        private static byte[] ReadPayload(DataStreamReader reader)
        {
            var payload = new byte[reader.Length];
            for (var i = 0; i < payload.Length; i++)
                payload[i] = reader.ReadByte();

            return payload;
        }
    }
}
