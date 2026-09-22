using System;
using Steamworks;
using UnityEngine;

public class SteamLobby : LobbySystem
{
    protected override string SystemTag => "SteamLobby";
    public CSteamID CurrentLobbyId { get; private set; }
    public bool HasLobby => CurrentLobbyId.IsValid();
    public event Action<CSteamID> LobbyCreated;
    public event Action<CSteamID> LobbyJoined;
    public event Action<EResult> LobbyCreateFailed;
    
    
    private Callback<LobbyCreated_t> m_LobbyCreated;
    private Callback<LobbyEnter_t> m_LobbyEntered;
    private Callback<GameLobbyJoinRequested_t> m_JoinRequested;

    public override void OnInit()
    {
        base.OnInit();
        m_LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);

        m_LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        m_JoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
    }
    
    // 创建房间
    public override void CreateRoom(int maxPlayers)
    {
        if (!SteamAPI.IsSteamRunning())
        {
            Log.Error(SystemTag,"Steam 未运行，无法创建房间。");
            return;
        }

        ELobbyType lobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;
        maxPlayers = Math.Max(1, maxPlayers);
        // 异步请求，最终结果由 OnLobbyCreated 返回。
        SteamMatchmaking.CreateLobby(lobbyType, maxPlayers);
    }
    
    // 创建房间回调
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            LobbyCreateFailed?.Invoke(callback.m_eResult);
            Log.Error(SystemTag, $"创建 Steam 房间失败：{callback.m_eResult}");
            return;
        }

        CurrentLobbyId  = new CSteamID(callback.m_ulSteamIDLobby);
        Log.Info(SystemTag,$"Lobby 房间创建成功！ 房间号:{CurrentLobbyId}");
        SteamMatchmaking.SetLobbyData(CurrentLobbyId, "name", $"{SteamFriends.GetPersonaName()} 的房间");
        SteamMatchmaking.SetLobbyData(CurrentLobbyId, "version", Application.version);
        SteamMatchmaking.SetLobbyData(CurrentLobbyId, "state", "Lobby");
        var gameNet = GameRoot.Instance.GameNet;
        if (!gameNet.IsNetworkRunning && !gameNet.StartHost())
        {
            Log.Error(SystemTag, "NGO Host 启动失败。");
            SteamMatchmaking.LeaveLobby(CurrentLobbyId);
            CurrentLobbyId = default;
            return;
        }
        SteamMatchmaking.SetLobbyJoinable(CurrentLobbyId, true);
        LobbyCreated?.Invoke(CurrentLobbyId);
    }
    
    /// <summary>
    /// 加入房间
    /// </summary>
    /// <param name="lobbyId"></param>
    public override void JoinRoom(ulong lobbyId)
    {
        JoinRoom(new CSteamID(lobbyId));
    }
    
    public void JoinRoom(CSteamID lobbyId)
    {
        if (!SteamAPI.IsSteamRunning())
        {
            Log.Error(SystemTag, "Steam 未运行。");
            return;
        }
        if (!lobbyId.IsValid())
        {
            Log.Error(SystemTag, "Lobby ID 无效。");
            return;
        }
        SteamMatchmaking.JoinLobby(lobbyId);
    }
    
    // 请求加入房间回调
    private void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    
    // 玩家进入房间回调
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        var response = (EChatRoomEnterResponse)callback.m_EChatRoomEnterResponse;
        if (response != EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            Log.Error(SystemTag, $"进入 Steam 房间失败：{response}");
            return;
        }
        CurrentLobbyId  = new CSteamID(callback.m_ulSteamIDLobby);
        CSteamID owner = SteamMatchmaking.GetLobbyOwner(CurrentLobbyId);
        LobbyJoined?.Invoke(CurrentLobbyId);
        // 创建房间后，房主自身也会进入 Lobby。
        if (owner == SteamUser.GetSteamID())
        {
            return; // 自己就是房主
        }
        if (!GameRoot.Instance.GameNet.StartClient(owner.m_SteamID))
        {
            Log.Error(SystemTag, "连接房主失败。");
            LeaveRoom();
        }
    }
    
    // 离开房间
    public override void LeaveRoom()
    {
        if (CurrentLobbyId.IsValid())
        {
            SteamMatchmaking.LeaveLobby(CurrentLobbyId);
            Log.Info(SystemTag,$"离开房间:{CurrentLobbyId}");
            CurrentLobbyId = default;
        }
    }

    public void OpenInvitePanel()
    {
        SteamSdk.OpenInviteDialog(CurrentLobbyId);
    }

    public override void OnDispose()
    {
        LeaveRoom();
        m_LobbyCreated?.Dispose();
        m_LobbyEntered?.Dispose();
        m_JoinRequested?.Dispose();
        m_LobbyCreated = null;
        m_LobbyEntered = null;
        m_JoinRequested = null;
        base.OnDispose();
    }
}