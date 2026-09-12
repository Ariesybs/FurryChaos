using System;
using System.Collections.Generic;

public class ServerSession : NetSession
{
    public bool IsRunning => m_Transport.IsRunning;
    public event Action<long> ClientConnected;
    public event Action<long> ClientDisconnected;
    
    private readonly IServerTransport m_Transport;
    private const ushort port = 7777;
    public ServerSession()
    {
        m_Transport = new UnityServerTransport();
        m_Transport.ClientConnected += OnClientConnected;
        m_Transport.ClientDisconnected += OnClientDisconnected;
        m_Transport.DataReceived += OnDataReceived;
    }

    public override void Init()
    {
        if (Listen(port))
        {
            Log.Info($"服务器启动成功，正在监听 UDP {port}");
        }
        else
        {
            Log.Error($"服务器启动失败，无法监听 UDP {port}");
        }
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
        Log.Info($"客户端连接：{connectionId}");
        ClientConnected?.Invoke(connectionId);
    }
    private void OnClientDisconnected(int connectionId)
    {
        Log.Info($"客户端断开：{connectionId}");
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