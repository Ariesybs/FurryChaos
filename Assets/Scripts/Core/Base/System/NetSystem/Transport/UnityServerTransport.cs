using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;

public class UnityServerTransport : IServerTransport
{
    private struct ClientConnection
    {
        public int Id;
        public NetworkConnection Connection;
    }
    private readonly List<ClientConnection> m_Connections = new();
    private NetworkDriver m_Driver;
    private int m_NextConnectionId = 1;
    public bool IsRunning { get; private set; }
    public event Action<int> ClientConnected;
    public event Action<int> ClientDisconnected;
    public event Action<int, byte[]> DataReceived;
    
    public bool Listen(ushort port)
    {
        Stop();
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndpoint.AnyIpv4.WithPort(port);
        if (m_Driver.Bind(endpoint) != 0)
        {
            m_Driver.Dispose();
            m_Driver = default;
            return false;
        }
        m_Driver.Listen();
        IsRunning = true;
        return true;
    }

    public void SendToClient(int connectionId, byte[] payload)
    {
        if (!IsRunning || payload == null || payload.Length == 0)
        {
            return;
        }
        for (var i = 0; i < m_Connections.Count; i++)
        {
            var client = m_Connections[i];
            if (client.Id != connectionId)
            {
                continue;
            }
            SendInternal(client.Connection, payload);
            return;
        }
    }

    public void Broadcast(byte[] payload)
    {
        if (!IsRunning || payload == null || payload.Length == 0)
        {
            return;
        }
        for (var i = 0; i < m_Connections.Count; i++)
        {
            SendInternal(m_Connections[i].Connection, payload);
        }
    }
    
    private void SendInternal(NetworkConnection connection, byte[] payload)
    {
        if (!connection.IsCreated)
        {
            return;
        }
        if (m_Driver.BeginSend(connection, out var writer) != 0)
        {
            return;
        }
        if (payload.Length > writer.Capacity)
        {
            m_Driver.AbortSend(writer);
            return;
        }
        for (var i = 0; i < payload.Length; i++)
        {
            writer.WriteByte(payload[i]);
        }
        m_Driver.EndSend(writer);
    }

    public void Poll()
    {
        if (!IsRunning || !m_Driver.IsCreated)
        {
            return;
        }
        m_Driver.ScheduleUpdate().Complete();
        AcceptPendingConnections();
        PollClientConnections();
    }
    
    private void AcceptPendingConnections()
    {
        NetworkConnection connection;
        while ((connection = m_Driver.Accept()) != default)
        {
            var client = new ClientConnection
            {
                Id = m_NextConnectionId++,
                Connection = connection
            };
            m_Connections.Add(client);
            ClientConnected?.Invoke(client.Id);
        }
    }
    private void PollClientConnections()
    {
        for (var i = m_Connections.Count - 1; i >= 0; i--)
        {
            var client = m_Connections[i];
            NetworkEvent.Type eventType;
            while ((eventType = m_Driver.PopEventForConnection(client.Connection, out var reader)) != NetworkEvent.Type.Empty)
            {
                switch (eventType)
                {
                    case NetworkEvent.Type.Data:
                    {
                        DataReceived?.Invoke(client.Id, ReadPayload(reader));
                        break;
                    }
                    case NetworkEvent.Type.Disconnect:
                    {
                        m_Connections.RemoveAt(i);
                        ClientDisconnected?.Invoke(client.Id);
                        break;
                    }
                }
                if (eventType == NetworkEvent.Type.Disconnect)
                {
                    break;
                }
            }
        }
    }

    
    private static byte[] ReadPayload(DataStreamReader reader)
    {
        var payload = new byte[reader.Length];
        for (var i = 0; i < payload.Length; i++)
        {
            payload[i] = reader.ReadByte();
        }
        return payload;
    }
    
    public void Stop()
    {
        if (m_Driver.IsCreated)
        {
            for (var i = 0; i < m_Connections.Count; i++)
            {
                var connection = m_Connections[i].Connection;
                if (connection.IsCreated)
                {
                    m_Driver.Disconnect(connection);
                }
            }
            m_Driver.Dispose();
        }
        m_Driver = default;
        m_Connections.Clear();
        m_NextConnectionId = 1;
        IsRunning = false;
    }
    
    public void Dispose()
    {
        Stop();
    }
}