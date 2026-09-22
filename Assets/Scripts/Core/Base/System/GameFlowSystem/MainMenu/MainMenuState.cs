public class MainMenuState : GameFlowState
{
    public MainMenuState(GameFlowSystem flow) : base(flow)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        UIMgr.Open<UIMainMenuCtrl>();
    }
}