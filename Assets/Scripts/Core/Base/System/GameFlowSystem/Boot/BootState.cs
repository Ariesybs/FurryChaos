/// <summary>
/// 初始化服务。
/// </summary>
public class BootState : GameFlowState
{
    public BootState(GameFlowSystem flow) : base(flow)
    {
        
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Flow.ChangeState(new MainMenuState(Flow));
    }
}