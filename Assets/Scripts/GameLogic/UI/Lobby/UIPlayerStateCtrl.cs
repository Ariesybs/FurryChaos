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
    private LobbyPlayerData m_Data;

    private void Awake()
    {
        m_SelfRect = GetComponent<RectTransform>();
    }

    public void Setup(LobbyPlayerData data,Vector3 infoPos, Camera lobbyCam = null)
    {
        UpdateDataInfo(data);
        InUsing = true;
        UIUtils.SetActive(this,true);
        SetupAnchoredPosition(infoPos,lobbyCam);
    }

    public void UpdateDataInfo(LobbyPlayerData data)
    {
        m_Data = data;
        UIUtils.SetActive(m_PlayerOwnerGo,m_Data.IsLobbyOwner);
        UIUtils.SetActive(m_PlayerOtherGo,!m_Data.IsLobbyOwner);
        if (m_Data.IsLobbyOwner)
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
        UIUtils.TmpSetText(m_OwnerName,m_Data.CharacterName.ToString());
    }

    private void SetupOtherInfo()
    {
        UIUtils.TmpSetText(m_OtherName,m_Data.CharacterName.ToString());
        var stateIcon = m_Data.IsReady ? m_InReadyIcon : m_NotReadyIcon;
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
