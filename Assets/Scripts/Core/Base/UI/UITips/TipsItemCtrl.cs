using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TipsItemCtrl : MonoBehaviour
{
    [SerializeField] private RectTransform m_Rect;
    [SerializeField] private TextMeshProUGUI m_Text;
    [SerializeField] private Image m_Icon;
    
    public RectTransform Rect => m_Rect != null ? m_Rect : (RectTransform)transform;
    private void Awake()
    {
        if (m_Rect == null)
        {
            m_Rect = (RectTransform)transform;
        }
    }

    private void Reset()
    {
        m_Rect = GetComponent<RectTransform>();
    }

    public void SetContent(in TipsInfo info)
    {
        if (m_Text != null)
        {
            m_Text.text = string.IsNullOrEmpty(info.KeyHint) ? info.Text : $"[{info.KeyHint}] {info.Text}";
        }
        if (m_Icon != null)
        {
            UIUtils.SetActive(m_Icon,info.Icon != null);
            if (info.Icon != null)
            {
                m_Icon.sprite = info.Icon;
            }
        }
    }
}