public class NetworkSystem : LogicSystem
{
    private NetSession m_NetSession;
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
    }
}