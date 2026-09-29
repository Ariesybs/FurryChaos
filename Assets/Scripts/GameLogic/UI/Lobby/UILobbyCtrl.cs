using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyCtrl : UIPanel
{
    public override UIDisplayMode DisplayMode => UIDisplayMode.FullScreen;
    public Button m_BackBtn;
    public Button m_ReadyBtn;
    public Button m_StartBtn;
    public TextMeshProUGUI m_ReadyText;
    public TextMeshProUGUI m_LobbyMemberCountText;
    public UIPlayerStateCtrl m_PlayerStateInfoTemplate;
    public GameObject m_ReadyTipsGo;
    private LobbySystem m_Lobby;
    private LobbySession m_Session;
    private LobbyPlayerData m_LobbyPlayerData;
    private readonly List<UIPlayerStateCtrl> m_PlayerStateInfoList = new();
    private readonly Dictionary<ulong, UIPlayerStateCtrl> m_PlayerStateInfoDict = new();

    private void Awake()
    {
        m_Lobby = GameRoot.Instance.GameFlow.LobbySystem;
        if (m_Lobby.LobbySession != null)
        {
            OnLobbySessionReady(); // session已创建完成
        }
        else
        {
            m_Lobby.OnLobbySessionReady += OnLobbySessionReady; // 等待session创建
        }
        
        GameEvent.Register<LobbyStageModelSpawn>(OnPlayerModelSpawn);
        GameEvent.Register<LobbyStageModelDespawn>(OnPlayerModelDespawn);
    }

    private void OnLobbySessionReady()
    {
        m_Session = m_Lobby.LobbySession;
        InstantiatePlayerInfoTemplate(m_Session.GetLobbyMaxMemberCount());
        m_Session.OnLobbyChanged += OnLobbyChanged;
        UpdateRoomInfo();
    }

    void Start()
    {
        UIUtils.ButtonBindListener(m_BackBtn, OnBackClick);
        UIUtils.ButtonBindListener(m_ReadyBtn, OnReadyClick);
        UIUtils.ButtonBindListener(m_StartBtn, OnStartGameClick);
    }

    private void OnLobbyChanged()
    {
        UpdateRoomInfo();
    }
    

    private void UpdateRoomInfo()
    {
        UpdateReadyState();
        UpdateMemberText();
        UpdatePlayerState();
    }

    private void UpdateReadyState()
    {
        var localPlayerData = m_Session.GetLocalPlayerLobbyData();
        var isReady = GetLocalPlayerReadyState();
        var allPlayerReady = IsAllPlayerReady();
        var text = isReady ? "取消准备" : "准备";
        UIUtils.TmpSetText(m_ReadyText,text);
        UIUtils.SetActive(m_StartBtn,localPlayerData.IsLobbyOwner && allPlayerReady);
        UIUtils.SetActive(m_ReadyTipsGo,localPlayerData.IsLobbyOwner && !allPlayerReady);
        UIUtils.SetActive(m_ReadyBtn,!localPlayerData.IsLobbyOwner);
    }

    private bool IsAllPlayerReady()
    {
        var lobbyPlayers = m_Session.GetLobbyPlayers();
        var allReady = true;
        foreach (var player in lobbyPlayers)
        {
            if (player.IsLobbyOwner)
            {
                continue; // 房主默认准备
            }

            if (!player.IsReady)
            {
                allReady = false;
                break;
            }
        }

        return allReady;
    } 

    private void UpdateMemberText()
    {
        var memberMax = m_Session.GetLobbyMaxMemberCount();
        var curMemberCount = m_Session.GetLobbyPlayers().Count;
        UIUtils.TmpSetText(m_LobbyMemberCountText,$"{curMemberCount}/{memberMax}");
    }

    private void UpdatePlayerState()
    {
        foreach (var infoPair in m_PlayerStateInfoDict)
        {
            var data = m_Session.GetLobbyPlayerById(infoPair.Key);
            infoPair.Value.UpdateDataInfo(data);
        }
    }

    private void OnPlayerModelSpawn(LobbyStageModelSpawn msg)
    {
        if (m_PlayerStateInfoDict.ContainsKey(msg.ClientId))
        {
            return;
        }

        var stateCtrl = GetIdleStateCtrl();
        if (stateCtrl == null)
        {
            return;
        }
        
        var playerData = m_Session.GetLobbyPlayerById(msg.ClientId);
        
        stateCtrl.Setup(playerData,msg.InfoPos,msg.LobbyCam);
        
        m_PlayerStateInfoDict.Add(msg.ClientId,stateCtrl);
    }

    private void OnPlayerModelDespawn(LobbyStageModelDespawn msg)
    {
        if (m_PlayerStateInfoDict.TryGetValue(msg.ClientId, out var infoCtrl))
        {
            if (infoCtrl.InUsing)
            {
                infoCtrl.Hide();
            }
        }
    }

    private UIPlayerStateCtrl GetIdleStateCtrl()
    {
        foreach (var stateCtrl in m_PlayerStateInfoList)
        {
            if (!stateCtrl.InUsing)
            {
                return stateCtrl;
            }
        }

        return null;
    }

    private void InstantiatePlayerInfoTemplate(int count)
    {
        if (m_PlayerStateInfoTemplate != null)
        {
            for (var i = 0; i < count; i++)
            {
                var infoCtrl = Instantiate(m_PlayerStateInfoTemplate,m_PlayerStateInfoTemplate.transform.parent);
                UIUtils.SetActive(infoCtrl,false);
                m_PlayerStateInfoList.Add(infoCtrl);
            }
        }
    }

    private void OnBackClick()
    {
        GameRoot.Instance.GameFlow.LobbySystem.LeaveRoom();
    }

    private void OnReadyClick()
    {
        var isReady = GetLocalPlayerReadyState();
        m_Lobby.SetReady(!isReady);
    }

    private void OnStartGameClick()
    {
        GameRoot.Instance.GameFlow.LobbySystem.StartGame();
    }

    private bool GetLocalPlayerReadyState()
    {
        var playerData = m_Session.GetLocalPlayerLobbyData();
        return playerData.IsReady;
    }
    
    private void OnDestroy()
    {
        if (m_Lobby != null)
        {
            m_Lobby.OnLobbySessionReady -= OnLobbySessionReady;
        }
        if (m_Session != null)
        {
            m_Session.OnLobbyChanged -= OnLobbyChanged;
        }
        
        GameEvent.Unregister<LobbyStageModelSpawn>(OnPlayerModelSpawn);
        GameEvent.Unregister<LobbyStageModelDespawn>(OnPlayerModelDespawn);
    }
}
