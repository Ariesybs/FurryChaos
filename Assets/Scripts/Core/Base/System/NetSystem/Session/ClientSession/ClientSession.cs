using System;
using UnityEngine;

public class ClientSession : NetSession
{
    public bool IsRunning => m_Transport.IsRunning;
    public long LocalEntityId => m_LocalEntityId;
    
    public event Action Connected;
    public event Action Disconnected;
    
    private readonly IClientTransport m_Transport;
    private readonly int m_ServerTickRate;
    private long m_LocalEntityId;
    private uint m_LatestServerTick;
    private double m_LatestTickReceiveTime;
    private bool m_HasReceivedSnapshot;
    
    public double EstimatedServerTick =>
        !m_HasReceivedSnapshot ? 0 : 
            m_LatestServerTick + (Time.unscaledTimeAsDouble - m_LatestTickReceiveTime) * m_ServerTickRate;

    public ClientSession()
    {
        m_Transport = new UnityClientTransport();
        m_ServerTickRate = 30;
        m_Transport.Connected += OnConnected;
        m_Transport.Disconnected += OnDisconnected;
        m_Transport.DataReceived += OnDataReceived;
    }
    
    public bool Connect(string address, ushort port)
    {
        ResetState();
        return m_Transport.Connect(address, port);
    }

    public override void Init()
    {
        Connect("127.0.0.1",7777);
    }

    public override void Poll()
    {
        m_Transport.Poll();
    }
    
    private void OnConnected()
    {
        Log.Debug("客户端链接成功");
        Connected?.Invoke();
    }
    private void OnDisconnected()
    {
        ResetState();
        Disconnected?.Invoke();
    }

    private void OnDataReceived(byte[] payload)
    {
        if (payload == null || payload.Length == 0)
        {
            return;
        }
        ClientMsgRouter.HandleNetworkMsg(payload);
    }

    public override void Dispose()
    {
        m_Transport.Connected -= OnConnected;
        m_Transport.Disconnected -= OnDisconnected;
        m_Transport.DataReceived -= OnDataReceived;
        m_Transport.Dispose();
        ResetState();
    }
    
    private void ResetState()
    {
        m_LocalEntityId = 0;
        m_LatestServerTick = 0;
        m_LatestTickReceiveTime = 0;
        m_HasReceivedSnapshot = false;
    }
}