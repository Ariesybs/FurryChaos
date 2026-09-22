using System.Collections.Generic;

public class GameFlowSystem : LogicSystem
{
    private NetworkSystem m_Network;
    private GameFlowState m_State;
    public LoginSystem LoginSystem;
    public LobbySystem LobbySystem;
    public InGameSystem InGameSystem;
    public ResultSystem ResultSystem;
    
    private IGameFlowState m_CurrentState;
    public IGameFlowState CurrentState => m_CurrentState;

    public override void OnInit()
    {
        
        this.LoginSystem = new LoginSystem(this);
        this.LobbySystem = LobbySystem.Get(this);
        this.InGameSystem = new InGameSystem(this);
        this.ResultSystem = new ResultSystem(this);
        LoginSystem.OnInit();
        LobbySystem.OnInit();
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
        m_CurrentState?.OnUpdate(deltaTime);
    }
    
    public void ChangeState(IGameFlowState nextState)
    {
        if (nextState == null || ReferenceEquals(nextState, m_CurrentState))
        {
            return;
        }
        Log.Info("GameFlowSystem",$"Enter {nextState}");
        m_CurrentState?.OnExit();
        m_CurrentState = nextState;
        m_CurrentState.OnEnter();
    }
    
    
    public bool IsState<T>() where T : IGameFlowState
    {
        return m_CurrentState is T;
    }
    
}