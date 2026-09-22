using System;
using Steamworks;
using UnityEngine;

public abstract class LobbySystem : SubSystem
{
    protected LobbySystem(ISystem mainSystem) : base(mainSystem)
    {
        
    }

    protected override string SystemTag => "LobbySystem";
    public abstract void CreateRoom(int maxPlayer);
    public abstract void JoinRoom(ulong roomId);
    public abstract void LeaveRoom();
    public abstract void StartGame();

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