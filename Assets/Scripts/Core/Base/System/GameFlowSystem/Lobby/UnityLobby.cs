using System.Collections.Generic;
using Unity.Netcode;

public class UnityLobby : LobbySystem
{
    private GameFlowSystem FlowSys;
    private LobbySession m_Session;
    public UnityLobby(ISystem mainSystem) : base(mainSystem)
    {
        FlowSys = (GameFlowSystem)mainSystem;
    }

    private NetworkSystem GameNet => GameRoot.Instance.GameNet;

    protected override string SystemTag => "UnityLobby";

    public override void OnInit()
    {
        base.OnInit();
        if (GameNet != null)
        {
            GameNet.OnServerStart += OnServerStart;
            GameNet.OnClientStart += OnClientStart;
        }
    }

    // =================== 公开接口 =======================

    public override void CreateRoom(int maxPlayer)
    {
        if (!GameNet.IsNetworkRunning && !GameNet.StartHost())
        {
            Log.Info(SystemTag, "主机启动失败");
            return;
        }
    }

    public override void JoinRoom(ulong roomId)
    {
        if (!GameNet.IsNetworkRunning && !GameNet.StartClient())
        {
            Log.Info(SystemTag, "客户端启动失败");
            return;
        }
    }

    public override void SetReady(bool ready)
    {
        if (LobbySession != null)
        {
            LobbySession.SetReadyServerRpc(ready);
        }
    }

    public override void LeaveRoom()
    {
        GameNet.Stop();
        FlowSys.ChangeState(new MainMenuState(FlowSys));
    }

    public override void StartGame()
    {
        if (!GameNet.IsServer && GameNet.IsNetworkRunning)
        {
            // 只有房主才能开始游戏
            return;
        }
        FlowSys.ChangeState(new LoadingState(FlowSys));
    }

    // ========================= 私有方法 =============================
    
    private void OnServerStart()
    {
        if (GameNet.IsServer)
        {
            FlowSys.ChangeState(new LobbyState(FlowSys));
        }
    }
    
    private void OnClientStart()
    {
        if (!GameNet.IsServer)
        {
            FlowSys.ChangeState(new LobbyState(FlowSys));
        }
    }

    public override void OnDispose()
    {
        base.OnDispose();
        if (GameNet != null)
        {
            GameNet.OnServerStart -= OnServerStart;
            GameNet.OnClientStart -= OnClientStart;
        }
    }
}