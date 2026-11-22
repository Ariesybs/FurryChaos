public sealed class InGameState : GameFlowState
{
    private UIMainInGameCtrl InGamePanel;
    private bool m_ResultEntered;
    public InGameState(GameFlowSystem flow) : base(flow)
    {
    }

    public override void OnEnter()
    {
        InGamePanel = UIMgr.Open<UIMainInGameCtrl>();
        // 禁用中文输入
        GameIme.Disable();
        GameRoot.Instance.GamePlay.MatchSystem.ServerStartMatch();
    }
    
    public override void OnUpdate(float deltaTime)
    {
        if (m_ResultEntered)
        {
            return;
        }
        var session = Flow.InGameSystem.Session;
        if (session == null || session.Phase != MatchPhase.Ended)
        {
            return;
        }
        m_ResultEntered = true;
        Flow.ChangeState(new ResultState(Flow));
    }

    public override void OnExit()
    {
        // 启用中文输入
        GameIme.Restore();
    }
}