using System;
using System.Collections.Generic;
using Unity.Netcode;

public class LobbySession : NetworkBehaviour
{
    public event Action<ulong> OnPlayerJoined;
    public event Action<ulong> OnPlayerLeaved; 
    public event Action OnLobbyChanged;
    private NetworkSystem GameNet;
    private const int MAX_PLAYERS = 4;
    private readonly NetworkList<LobbyPlayerData> m_LobbyPlayers = new();
    private NetworkObject m_NetObj;
        
    private void Awake()
    {
        GameNet = GameRoot.Instance.GameNet;
        if (GameNet != null && GameNet.IsServer)
        {
            m_NetObj = GetComponent<NetworkObject>();
            m_NetObj?.Spawn();
        }
        DontDestroyOnLoad(this);
    }

    public override void OnNetworkSpawn()
    {
        m_LobbyPlayers.OnListChanged += OnListChanged;
        if (IsServer)
        {
            GameNet.OnClientConnected += OnClientConnected;
            GameNet.OnClientDisconnected += OnClientDisconnected;
            InitPlayer();
        }

        GameRoot.Instance.GameFlow.LobbySystem.RegisterSession(this);
    }

    private void InitPlayer()
    {
        var playerIds = GameNet.GetConnectionIdList();
        foreach (var playerId in playerIds)
        {
            AddPlayerToLobby(playerId);
        }
    }
    
    private void OnClientConnected(ulong playerId)
    {
        // 只由 Server 写 NetworkList，UI 统一听 LobbySession.OnPlayersChanged
        if (!GameNet.IsServer)
        {
            return;
        }

        AddPlayerToLobby(playerId);
    }
    
    private void OnClientDisconnected(ulong playerId)
    {
        if (!GameNet.IsServer)
        {
            return;
        }

        RemovePlayerFromLobby(playerId);
    }
    
    private void OnListChanged(NetworkListEvent<LobbyPlayerData> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<LobbyPlayerData>.EventType.Add:
                OnPlayerJoined?.Invoke(changeEvent.Value.ClientId);
                break;
            case NetworkListEvent<LobbyPlayerData>.EventType.Remove:
                OnPlayerLeaved?.Invoke(changeEvent.Value.ClientId);
                break;
        }
        OnLobbyChanged?.Invoke();
    }

    private void AddPlayerToLobby(ulong clientId)
    {
        if (m_LobbyPlayers.Count >= MAX_PLAYERS)
        {
            // 人数已满，拒绝连接
            NetworkManager.Singleton.DisconnectClient(clientId);
            return;
        }

        var idx = GetPlayerIdx(clientId);
        if (idx != -1)
        {
            // 已添加过玩家
            return;
        }

        var newPlayer = new LobbyPlayerData()
        {
            ClientId = clientId,
            PlayerName = $"Player_{clientId}",
            IsReady = false,
            IsLobbyOwner = clientId == NetworkManager.ServerClientId,
            CharacterName = $"Cat_{clientId}"
        };
        m_LobbyPlayers.Add(newPlayer);
    }

    private void RemovePlayerFromLobby(ulong clientId)
    {
        if (m_LobbyPlayers == null || m_LobbyPlayers.Count == 0)
        {
            return;
        }

        foreach (var player in m_LobbyPlayers)
        {
            if (player.ClientId == clientId)
            {
                m_LobbyPlayers.Remove(player);
                break;
            }
        }
    }

    public NetworkList<LobbyPlayerData> GetLobbyPlayers()
    {
        return m_LobbyPlayers;
    }

    public LobbyPlayerData GetLobbyPlayerById(ulong clientId)
    {
        foreach (var player in m_LobbyPlayers)
        {
            if (player.ClientId == clientId)
            {
                return player;
            }
        }

        return default;
    }

    public LobbyPlayerData GetLocalPlayerLobbyData()
    {
        if (NetworkManager.Singleton == null)
        {
            return default;
        }
        var idx = GetPlayerIdx(NetworkManager.Singleton.LocalClientId);
        if (idx < 0)
        {
            return default;
        }
        return m_LobbyPlayers[idx];
    }

    public int GetLobbyMaxMemberCount()
    {
        return MAX_PLAYERS;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetReadyServerRpc(bool ready, ServerRpcParams rpcParams = default)
    {
        if (!IsServer)
        {
            return;
        }

        ulong clientId = rpcParams.Receive.SenderClientId;
        var idx = GetPlayerIdx(clientId);
        if (idx < 0)
        {
            return;
        }

        var player = m_LobbyPlayers[idx];
        player.IsReady = ready;
        m_LobbyPlayers[idx] = player;
    }
    
    [ClientRpc]
    public void StartGameClientRpc()
    {
        GameRoot.Instance.GameFlow.ChangeState(new LoadingState(GameRoot.Instance.GameFlow));
    }
    
    // ====================== 内部工具 ========================
    private int GetPlayerIdx(ulong clientId)
    {
        var idx = -1;
        for (var i = 0; i < m_LobbyPlayers.Count; i++)
        {
            var player = m_LobbyPlayers[i];
            if (player.ClientId == clientId)
            {
                idx = i;
                break;
            }
        }
        return idx;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            GameNet.OnClientConnected -= OnClientConnected;
            GameNet.OnClientDisconnected -= OnClientDisconnected;
        }
        m_LobbyPlayers.OnListChanged -= OnListChanged;
        GameRoot.Instance.GameFlow.LobbySystem.UnregisterSession();
    }
}
