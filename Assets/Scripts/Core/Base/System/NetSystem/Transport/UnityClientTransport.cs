using System;
using Unity.Collections;
using Unity.Networking.Transport;

public class UnityClientTransport : IClientTransport
{
    public bool IsRunning { get; private set; }
    public event Action Connected;
    public event Action Disconnected;
    public event Action<byte[]> DataReceived;
    
    private NetworkDriver m_Driver;
    private NetworkConnection m_Connection;
    
    public bool Connect(string address, ushort port)
    {
        Stop(); // 重连
        try
        {
            m_Driver = NetworkDriver.Create();
            var endpoint = NetworkEndpoint.Parse(address, port); // 拼接ip地址
            m_Connection = m_Driver.Connect(endpoint);
            IsRunning = m_Connection.IsCreated;
            return IsRunning;
        }
        catch (Exception e)
        {
            if (m_Driver.IsCreated)
            {
                m_Driver.Dispose();
            }
            m_Driver = default;
            m_Connection = default;
            IsRunning = false;
            return false;
        }
    }

    public void SendToServer(byte[] payload)
    {
        if (!IsRunning || payload == null || payload.Length == 0 || !m_Connection.IsCreated)
        {
            return;
        }

        SendInternal(m_Connection, payload);
    }
    
    private void SendInternal(NetworkConnection connection, byte[] payload)
    {
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
        if (!IsRunning || !m_Driver.IsCreated || !m_Connection.IsCreated)
        {
            return;
        }
        m_Driver.ScheduleUpdate().Complete();
        NetworkEvent.Type eventType;
        while ((eventType = m_Connection.PopEvent(m_Driver, out var reader)) != NetworkEvent.Type.Empty)
        {
            switch (eventType)
            {
                case NetworkEvent.Type.Connect:
                    Connected?.Invoke();
                    break;
                case NetworkEvent.Type.Data:
                    DataReceived?.Invoke(ReadPayload(reader));
                    break;
                case NetworkEvent.Type.Disconnect:
                    m_Connection = default;
                    IsRunning = false;
                    Disconnected?.Invoke();
                    return;
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
            if (m_Connection.IsCreated)
            {
                m_Driver.Disconnect(m_Connection);
            }
            m_Driver.Dispose();
        }
        m_Driver = default;
        m_Connection = default;
        IsRunning = false;
    }
    
    public void Dispose()
    {
        Stop();
    }
}