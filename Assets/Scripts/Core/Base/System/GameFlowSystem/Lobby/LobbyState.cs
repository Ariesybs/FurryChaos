using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public sealed class LobbyState : GameFlowState
{
    private readonly AssetLoader m_Loader;
    private readonly NetworkSystem m_GameNet; 
    private GameObject m_LobbyStageGo;
    private GameObject m_LobbySessionGo;
    private UIPanel m_LobbyUI;
    private const string STAGE = "LobbyStage";
    private const string SESSION = "LobbySession";
    public LobbyState(GameFlowSystem flow) : base(flow)
    {
        m_Loader = GameRoot.Instance.GameLoader.AssetLoader;
        m_GameNet = GameRoot.Instance.GameNet;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        m_GameNet.OnClientStop += OnClientStop;
        // 生成会话
        if (GameRoot.Instance.GameNet.IsServer)
        {
            LoadLobbySession().Forget();
        }
        // 生成组队面板
        m_LobbyUI = UIMgr.Open<UILobbyCtrl>();
        // 生成组队场景
        LoadLobbyStage().Forget();
    }

    private void OnClientStop(bool isHost)
    {
        // 与服务器断开连接，返回主菜单
        if (!isHost)
        {
            Flow.ChangeState(new MainMenuState(Flow));
        }
    }

    private async UniTaskVoid LoadLobbyStage()
    {
        var prefab = await m_Loader.LoadPrefab(STAGE);
        m_LobbyStageGo = Object.Instantiate(prefab);
    }

    private async UniTaskVoid LoadLobbySession()
    {
        var prefab = await m_Loader.LoadPrefab(SESSION);
        m_LobbySessionGo = Object.Instantiate(prefab);
    }

    public override void OnExit()
    {
        base.OnExit();
        m_Loader.ReleasePrefab(STAGE,m_LobbyStageGo);
        // Object.Destroy(m_LobbySessionGo);
        UIMgr.Close(m_LobbyUI);
    }
}