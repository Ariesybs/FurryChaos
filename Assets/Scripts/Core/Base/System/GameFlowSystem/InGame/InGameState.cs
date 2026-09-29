public sealed class InGameState : GameFlowState
{
    private UIMainInGameCtrl InGamePanel;
    public InGameState(GameFlowSystem flow) : base(flow)
    {
    }

    public override void OnEnter()
    {
        InGamePanel = UIMgr.Open<UIMainInGameCtrl>();
        // 禁用中文输入
        GameIme.Disable();
    }

    public override void OnExit()
    {
        // 启用中文输入
        GameIme.Restore();
    }
}