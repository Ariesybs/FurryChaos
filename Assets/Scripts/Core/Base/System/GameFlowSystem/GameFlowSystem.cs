using System.Collections.Generic;

public class GameFlowSystem : LogicSystem
{
    private NetworkSystem m_Network;
    private GameFlowState m_State;
    public LoginSystem LoginSystem;
    public LobbySystem LobbySystem;
    public LoadingSystem LoadingSystem;
    public InGameSystem InGameSystem;
    public ResultSystem ResultSystem;
    
    private IGameFlowState m_CurrentState;
    public IGameFlowState CurrentState => m_CurrentState;

    public override void OnInit()
    {
        
        this.LoginSystem = new LoginSystem();
        this.LobbySystem = LobbySystem.Get();
        this.LoadingSystem = new LoadingSystem();
        this.InGameSystem = new InGameSystem();
        this.ResultSystem = new ResultSystem();
        LoginSystem.OnInit();
        LobbySystem.OnInit();
        LoadingSystem.OnInit();
        InGameSystem.OnInit();
        ResultSystem.OnInit();
        base.OnInit();
        ChangeState(new BootState(this));
    }

    public override void OnAfterAllSystemInit()
    {
        m_Network = GameRoot.Instance.GameNet;
    }
    
    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        m_CurrentState?.Update(deltaTime);
    }
    
    public void ChangeState(IGameFlowState nextState)
    {
        if (nextState == null || ReferenceEquals(nextState, m_CurrentState))
        {
            return;
        }
        m_CurrentState?.Exit();
        m_CurrentState = nextState;
        m_CurrentState.Enter();
    }
    
    
    public bool IsState<T>() where T : IGameFlowState
    {
        return m_CurrentState is T;
    }
    
}