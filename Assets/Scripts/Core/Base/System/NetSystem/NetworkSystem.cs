using System;
using UnityEngine;

[Serializable]
public class NetworkSystem : LogicSystem
{
    public NetworkConfig Config;
    private NetSession m_NetSession;
    [HideInInspector] 
    public int ConnectionId;
    public override void OnInit()
    {
        base.OnInit();
        
#if UNITY_SERVER
        m_NetSession = new ServerSession();
#else
        m_NetSession = new ClientSession();
#endif
    }

    public override void OnAfterAllSystemInit()
    {
        base.OnAfterAllSystemInit();
        m_NetSession.Init();
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        m_NetSession.Poll();
    }

    public override void OnDispose()
    {
        base.OnDispose();
        m_NetSession.Dispose();
        ConnectionId = 0;
    }

    public void Send(NetworkMsg msg)
    {
        m_NetSession?.Send(msg);
    }

    public void Send(int connectionId, NetworkMsg msg)
    {
        m_NetSession?.Send(connectionId,msg);
    }
}