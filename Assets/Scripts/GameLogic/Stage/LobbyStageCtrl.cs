using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LobbyStageCtrl : MonoBehaviour
{
    public List<Transform> m_CatStandPosList;
    public Camera m_LobbyCamera;
    public Vector3 m_UIInfoOffset;
    private bool[] m_ContainerIsUsed;
    private readonly Dictionary<ulong, Transform> m_PlayerCatDict = new();
    private LobbySystem m_Lobby;
    private LobbySession m_Session;
    private const string CAT_PREVIEW = "PreviewCat";
    private void Awake()
    {
        m_Lobby = GameRoot.Instance.GameFlow.LobbySystem;
        m_ContainerIsUsed = new bool[m_CatStandPosList.Count];
        if (m_Lobby.LobbySession != null)
        {
            OnLobbySessionReady(); // session已创建完成
        }
        else
        {
            m_Lobby.OnLobbySessionReady += OnLobbySessionReady; // 等待session创建
        }
    }
    
    private void OnLobbySessionReady()
    {
        m_Session = m_Lobby.LobbySession;
        m_Session.OnLobbyChanged += OnLobbyChanged;
        m_Session.OnPlayerJoined += OnPlayerJoined;
        m_Session.OnPlayerLeaved += OnPlayerLeaved;
        InitPreview();
    }
    
    private void OnLobbyChanged()
    {
        
    }
    
    private void OnPlayerJoined(ulong clientId)
    {
        AddCatPreviewToContainer(clientId);
    }

    private void OnPlayerLeaved(ulong clientId)
    {
        RemoveCatPreviewFromContainer(clientId);
    }
    private void InitPreview()
    {
        var players = m_Session.GetLobbyPlayers();
        foreach (var player in players)
        {
            AddCatPreviewToContainer(player.ClientId);
        }
    }

    private void AddCatPreviewToContainer(ulong clientId)
    {
        if (m_PlayerCatDict.ContainsKey(clientId))
        {
            return;
        }
        var container = GetNextEmptyContainer();
        if (container != null)
        {
            LoadCatPreview(clientId,container).Forget();
        }
    }

    private async UniTaskVoid LoadCatPreview(ulong clientId,Transform container)
    {
        var prefab = await GameRoot.Instance.GameLoader.AssetLoader.LoadPrefab(CAT_PREVIEW);
        if (prefab != null)
        {
            var go = Instantiate(prefab,container);
            m_PlayerCatDict.TryAdd(clientId, container);
            GameEvent.Trigger(new LobbyStageModelSpawn()
            {
                ClientId = clientId,
                InfoPos = container.position + m_UIInfoOffset,
                LobbyCam = m_LobbyCamera
            });
        }
    }

    private void RemoveCatPreviewFromContainer(ulong clientId)
    {
        if (m_PlayerCatDict.TryGetValue(clientId,out var container))
        {
            GameRoot.Instance.GameLoader.AssetLoader.ReleasePrefab(CAT_PREVIEW,container.GetChild(0).gameObject);
            var idx = m_CatStandPosList.IndexOf(container);
            if (idx != -1)
            {
                m_ContainerIsUsed[idx] = false;
            }
            m_PlayerCatDict.Remove(clientId);
            GameEvent.Trigger(new LobbyStageModelDespawn()
            {
                ClientId = clientId,
            });
        }
    }

    private Transform GetNextEmptyContainer()
    {
        for (var i = 0; i < m_ContainerIsUsed.Length; i++)
        {
            var isUsed = m_ContainerIsUsed[i];
            if (!isUsed)
            {
                m_ContainerIsUsed[i] = true;
                return m_CatStandPosList[i];
            }
        }

        return null;
    }

    private void OnDestroy()
    {
        if (m_Session != null)
        {
            m_Session.OnLobbyChanged -= OnLobbyChanged;
            m_Session.OnPlayerJoined -= OnPlayerJoined;
            m_Session.OnPlayerLeaved -= OnPlayerLeaved;
        }
    }
}

public struct LobbyStageModelSpawn : IMsg
{
    public ulong ClientId;
    public Vector3 InfoPos;
    public Camera LobbyCam;
}

public struct LobbyStageModelDespawn : IMsg
{
    public ulong ClientId;
}
