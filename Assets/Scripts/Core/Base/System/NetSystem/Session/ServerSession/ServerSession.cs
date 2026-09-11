using System;
using System.Collections.Generic;

public class ServerSession : NetSession
{
    public bool IsRunning => m_Transport.IsRunning;
    public event Action<long> ClientConnected;
    public event Action<long> ClientDisconnected;
    
    private readonly IServerTransport m_Transport;

    public ServerSession()
    {
        m_Transport = new UnityServerTransport();
        m_Transport.ClientConnected += OnClientConnected;
        m_Transport.ClientDisconnected += OnClientDisconnected;
        m_Transport.DataReceived += OnDataReceived;
    }

    public override void Init()
    {
        Listen(7777);
    }

    public bool Listen(ushort port)
    {
        ResetState();
        return m_Transport.Listen(port);
    }
    public override void Poll()
    {
        m_Transport.Poll();
    }
    
    public void Stop()
    {
        m_Transport.Stop();
        ResetState();
    }
    
    private void OnClientConnected(int connectionId)
    {
        ClientConnected?.Invoke(connectionId);
    }
    private void OnClientDisconnected(int connectionId)
    {
        ClientDisconnected?.Invoke(connectionId);
    }

    private void OnDataReceived(int connectionId, byte[] payload)
    {
        if (payload == null || payload.Length == 0)
        {
            return;
        }
        ServerMsgRouter.HandleNetworkMsg(connectionId,payload);
    }

    public override void Dispose()
    {
        m_Transport.ClientConnected -= OnClientConnected;
        m_Transport.ClientDisconnected -= OnClientDisconnected;
        m_Transport.DataReceived -= OnDataReceived;
        m_Transport.Dispose();
        ResetState();
    }
    
    private void ResetState()
    {
        
    }
}