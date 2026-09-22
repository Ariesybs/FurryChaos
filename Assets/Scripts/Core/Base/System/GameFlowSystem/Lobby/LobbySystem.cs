using System;
using Steamworks;
using UnityEngine;

public abstract class LobbySystem : SubSystem
{
    protected override string SystemTag => "LobbySystem";
    public abstract void CreateRoom(int maxPlayer);
    public abstract void JoinRoom(ulong roomId);
    public abstract void LeaveRoom();

    public static LobbySystem Get()
    {
        var transportMode = GameRoot.Instance.GameNet.CurTransportMode;
        if (transportMode == NetworkSystem.TransportMode.Unity)
        {
            return new UnityLobby();
        }
        else if (transportMode == NetworkSystem.TransportMode.Steam)
        {
            return new SteamLobby();
        }

        return null;
    }
}