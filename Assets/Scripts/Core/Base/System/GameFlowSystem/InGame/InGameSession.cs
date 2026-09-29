using System.Collections.Generic;
using Unity.Netcode;

public class InGameSession : NetworkBehaviour
{
    private readonly NetworkList<InGamePlayerData> m_Players = new();
    public NetworkList<InGamePlayerData> Players => m_Players;
    public NetworkVariable<bool> PlayersSpawned = new();

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);
        GameRoot.Instance.GameFlow.InGameSystem.RegisterSession(this);
    }

    public void ServerSetPlayers(List<InGamePlayerData> players)
    {
        if (!IsServer)
        {
            return;
        }

        m_Players.Clear();
        foreach (var player in players)
        {
            m_Players.Add(player);
        }
    }

    public void ServerMarkSpawned()
    {
        if (IsServer)
        {
            PlayersSpawned.Value = true;
        }
    }
}