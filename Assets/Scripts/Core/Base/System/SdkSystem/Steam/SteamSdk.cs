using Steamworks;
public class SteamSdk : SubSystem
{
    public override void OnInit()
    {
        base.OnInit();
        IsInitialized = SteamAPI.Init();
        if (!IsInitialized)
        {
            Log.Error("Steam 初始化失败，请确认 Steam 客户端已运行。");
            return;
        }
        var hostSteamId = SteamUser.GetSteamID().m_SteamID;
        Log.Info("SteamSdk",$"SteamSDK 初始化成功 Host Steam ID {hostSteamId}");
        SteamNetworkingUtils.InitRelayNetworkAccess();
    }
    
    /// <summary>
    /// 打开Steam的邀请面板
    /// </summary>
    /// <returns></returns>
    public static bool OpenInviteDialog(CSteamID lobbyId)
    {
        if (!lobbyId.IsValid())
        {
            Log.Error("LobbySystem", "当前没有有效房间。");
            return false;
        }

        SteamFriends.ActivateGameOverlayInviteDialog(lobbyId);

        return true;
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        if (!IsInitialized)
        {
            return;
        }
        SteamAPI.RunCallbacks();
    }

    public override void OnDispose()
    {
        base.OnDispose();
        if (!IsInitialized)
        {
            return;
        }
        SteamAPI.Shutdown();
        IsInitialized = false;
    }
}