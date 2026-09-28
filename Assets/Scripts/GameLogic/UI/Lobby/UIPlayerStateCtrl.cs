using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerStateCtrl : MonoBehaviour
{
    [Header("房主玩家")]
    [SerializeField]
    private GameObject m_PlayerOwnerGo;
    [SerializeField]
    private TextMeshProUGUI m_OwnerName;
    [Header("其他玩家")]
    [SerializeField]
    private GameObject m_PlayerOtherGo;
    [SerializeField]
    private TextMeshProUGUI m_OtherName;
    [SerializeField]
    private Image m_StateImage;
    [SerializeField]
    private Sprite m_InReadyIcon;
    [SerializeField]
    private Sprite m_NotReadyIcon;

    public bool InUsing = false;
    private RectTransform m_SelfRect;
    private PlayerLobbyData m_LobbyData;

    private void Awake()
    {
        m_SelfRect = GetComponent<RectTransform>();
    }

    public void Setup(PlayerLobbyData data,Vector3 infoPos, Camera lobbyCam = null)
    {
        UpdateDataInfo(data);
        InUsing = true;
        UIUtils.SetActive(this,true);
        SetupAnchoredPosition(infoPos,lobbyCam);
    }

    public void UpdateDataInfo(PlayerLobbyData data)
    {
        m_LobbyData = data;
        UIUtils.SetActive(m_PlayerOwnerGo,m_LobbyData.IsLobbyOwner);
        UIUtils.SetActive(m_PlayerOtherGo,!m_LobbyData.IsLobbyOwner);
        if (m_LobbyData.IsLobbyOwner)
        {
            SetupOwnerInfo();
        }
        else
        {
            SetupOtherInfo();
        }
    }

    private void SetupOwnerInfo()
    {
        UIUtils.TmpSetText(m_OwnerName,m_LobbyData.CharacterName.ToString());
    }

    private void SetupOtherInfo()
    {
        UIUtils.TmpSetText(m_OtherName,m_LobbyData.CharacterName.ToString());
        var stateIcon = m_LobbyData.IsReady ? m_InReadyIcon : m_NotReadyIcon;
        UIUtils.ImageSetIcon(m_StateImage,stateIcon);
    }

    private void SetupAnchoredPosition(Vector3 infoPos, Camera lobbyCam)
    {
        var parent = m_SelfRect.parent as RectTransform;
        if (lobbyCam == null || parent == null)
        {
            return;
        }
        var screen = lobbyCam.WorldToScreenPoint(infoPos);
        if (screen.z < 0f)
        {
            return;
        }
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, null, out Vector2 local))
        {
            m_SelfRect.anchoredPosition = local;
        }
    }

    public void Hide()
    {
        UIUtils.SetActive(this,false);
        InUsing = false;
    }
}
