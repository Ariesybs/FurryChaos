public sealed class LobbyState : GameFlowState
{
    public LobbyState(GameFlowSystem flow) : base(flow)
    {
    }

    public void StartGame()
    {
        if (!Network.IsServer)
            return;

        Flow.ChangeState(new LoadingState(Flow));
    }
}