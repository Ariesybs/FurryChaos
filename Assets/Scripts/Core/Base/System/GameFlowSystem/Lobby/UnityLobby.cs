public class UnityLobby : LobbySystem
{
    private GameFlowSystem FlowSys;
    public UnityLobby(ISystem mainSystem) : base(mainSystem)
    {
        FlowSys = (GameFlowSystem)mainSystem;
    }

    private NetworkSystem GameNet => GameRoot.Instance.GameNet;

    protected override string SystemTag => "UnityLobby";

    public override void CreateRoom(int maxPlayer)
    {
        if (!GameNet.IsNetworkRunning && !GameNet.StartHost())
        {
            Log.Info(SystemTag, "主机启动失败");
            return;
        }

        FlowSys.ChangeState(new LobbyState(FlowSys));
    }

    public override void JoinRoom(ulong roomId)
    {
        
    }

    public override void LeaveRoom()
    {
        if (GameNet.IsServer)
        {
            // 关闭服务器
            GameNet.StopHost();
        }
    }

    public override void StartGame()
    {
        if (!GameNet.IsServer)
        {
            // 只有房主才能开始游戏
            return;
        }
        FlowSys.ChangeState(new LoadingState(FlowSys));
    }
}