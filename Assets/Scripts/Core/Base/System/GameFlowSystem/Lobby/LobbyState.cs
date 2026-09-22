public sealed class LobbyState : GameFlowState
{
    public LobbyState(GameFlowSystem flow) : base(flow)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        // 生成组队面板
        UIMgr.Open<UILobbyCtrl>();
        // 生成组队场景
    }

    public void StartGame()
    {
        if (!Network.IsServer)
            return;

        Flow.ChangeState(new LoadingState(Flow));
    }
}