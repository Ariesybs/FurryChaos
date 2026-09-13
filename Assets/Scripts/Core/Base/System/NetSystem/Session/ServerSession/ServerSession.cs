using System;
using System.Collections.Generic;
using System.IO;

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
        if (Listen())
        {
            Log.Info($"服务器启动成功，正在监听 UDP {port}");
        }
        else
        {
            Log.Error($"服务器启动失败，无法监听 UDP {port}");
        }
    }

    public override void Send(int connectionId, NetworkMsg msg)
    {
        if (msg == null)
        {
            return;
        }

        var payload = msg.Encode();
        m_Transport.SendToClient(connectionId,payload);
    }

    private bool Listen()
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
        using var stream = new MemoryStream(payload, false);
        using var reader = new BinaryReader(stream);
        ServerMsgRouter.HandleNetworkMsg(connectionId,reader.ReadUInt16());
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