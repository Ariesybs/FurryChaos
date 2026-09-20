public sealed class InGameState : GameFlowState
{
    public InGameState(GameFlowSystem flow) : base(flow)
    {
    }

    public override void Enter()
    {
        Network.OnClientConnected += OnClientConnected;
        // Network.ClientDisconnected += OnClientDisconnected;
        // if (sceneName != SceneDefine.GameScene)
        //     return;
        // if (Network.IsServer)
        // {
        //     foreach (ulong clientId in clientsCompleted)
        //     {
        //         Flow.InGameSystem.SpawnPlayer(clientId);
        //     }
        // }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (Network.IsServer)
        {
            Flow.InGameSystem.SpawnPlayer(clientId);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // 更新玩家列表或检测房主退出
    }

    public override void Exit()
    {
        Network.OnClientConnected -= OnClientConnected;
        // Network.ClientDisconnected -= OnClientDisconnected;
    }
}