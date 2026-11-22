public sealed class ResultState : GameFlowState
{
    private UIPanel m_Panel;

    public ResultState(GameFlowSystem flow) : base(flow)
    {
    }

    public override void OnEnter()
    {
        // 把复制来的结算快照拷到本地，之后不再依赖网络对象的生命周期
        var resultInfo = Flow.ResultSystem.CaptureFromSession(Flow.InGameSystem.Session);
        m_Panel = UIMgr.Open<UIResultCtrl>(resultInfo);
    }

    public override void OnExit()
    {
        UIMgr.Close(m_Panel);
    }
}