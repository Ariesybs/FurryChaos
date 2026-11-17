using System.Collections.Generic;

public class PlayerMgr : NetworkSingleton<PlayerMgr>
{
    private readonly Dictionary<ulong, Player> m_PlayerDict = new();
    public void RegisterPlayer(ulong clientId,Player player)
    {
        m_PlayerDict.TryAdd(clientId, player);
    }

    public void UnregisterPlayer(ulong clientId)
    {
        m_PlayerDict.Remove(clientId);
    }

    public Player GetPlayer(ulong clientId)
    {
        m_PlayerDict.TryGetValue(clientId, out var player);
        return player;
    }

    public Player GetSelf()
    {
        return GetPlayer(NetworkManager.LocalClientId);
    }
}