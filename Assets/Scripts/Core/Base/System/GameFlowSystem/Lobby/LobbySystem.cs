using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public abstract class LobbySystem : SubSystem
{
    protected LobbySystem(ISystem mainSystem) : base(mainSystem)
    {
        
    }

    public LobbySession LobbySession;
    protected override string SystemTag => "LobbySystem";
    public abstract void CreateRoom(int maxPlayer);
    public abstract void JoinRoom(ulong roomId);
    public abstract void SetReady(bool ready);
    public abstract void LeaveRoom();
    public abstract void StartGame();
    public Action OnLobbySessionReady;
    public LobbySession GetSession()
    {
        return LobbySession;
    }
    public void RegisterSession(LobbySession session)
    {
        LobbySession = session;
        OnLobbySessionReady?.Invoke();
    }

    public void UnregisterSession()
    {
        LobbySession = null;
    }
    public Action OnRoomStateUpdate;
    public Action<ulong> OnPlayerEnter;
    public Action<ulong> OnPlayerExit; 

    public static LobbySystem Get(ISystem mainSystem)
    {
        var transportMode = GameRoot.Instance.GameNet.CurTransportMode;
        if (transportMode == NetworkSystem.TransportMode.Unity)
        {
            return new UnityLobby(mainSystem);
        }
        else if (transportMode == NetworkSystem.TransportMode.Steam)
        {
            return new SteamLobby(mainSystem);
        }

        return null;
    }
    
}