using System.Collections.Generic;

public class LoadingState : GameFlowState
{
    public LoadingState(GameFlowSystem flow) : base(flow)
    {
        
    }
    
    public override void Enter()
    {
        Network.LoadCompleted += OnLoadCompleted;
        if (Network.IsServer)
        {
            Network.LoadScene(SceneDefine.GameScene);
        }
    }
    private void OnLoadCompleted( string sceneName, IReadOnlyList<ulong> clientsCompleted)
    {
        Flow.ChangeState(new InGameState(Flow));
    }
    public override void Exit()
    {
        Network.LoadCompleted -= OnLoadCompleted;
    }
}