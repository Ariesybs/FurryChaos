using System;
using System.Collections.Generic;
using UnityEngine;

public class TipsMgr : MonoSingleton<TipsMgr>
{
    [SerializeField] private TipsItemCtrl m_ItemTemplate; // item 模板（保持隐藏）
    [SerializeField] private float m_EdgeMargin = 60f;     // 屏幕边缘钳制边距（像素）
    [SerializeField] private LayerMask m_OcclusionMask = ~0; // 遮挡检测层

    private readonly Dictionary<int, TipsInfo> m_Tips = new();
    private readonly Dictionary<int, TipsItemCtrl> m_Items = new();
    private Canvas m_Canvas;
    private Camera m_Cam;
    private RectTransform m_TipsRoot;
    
    protected override void Awake()
    {
        base.Awake();
        m_Canvas = GetComponentInParent<Canvas>()?.rootCanvas;
        HideTemplate();
        if (m_TipsRoot == null)
        {
            m_TipsRoot = (RectTransform)transform;
        }
    }
    private void Start()
    {
        m_Cam = Camera.main;
    }

    private void HideTemplate()
    {
        UIUtils.SetActive(m_ItemTemplate,false);
    }

    private void Reset()
    {
        m_TipsRoot = (RectTransform)transform;
    }

    public static void ShowTip(TipsInfo info) => Instance?.Show(info);
    public static void HideTip(int tipId) => Instance?.Hide(tipId);
    public static void HideAllTips() => Instance?.HideAll();

    /// <summary>同 TipId 覆盖即更新。</summary>
    public void Show(TipsInfo info) => m_Tips[info.TipId] = info;

    public void Hide(int tipId)
    {
        m_Tips.Remove(tipId);
        if (m_Items.Remove(tipId, out var item) && item != null)
        {
            Destroy(item.gameObject);
        }
    }

    public void HideAll()
    {
        m_Tips.Clear();
        foreach (var item in m_Items.Values)
        {
            if (item != null) Destroy(item.gameObject);
        }
        m_Items.Clear();
    }
    

    protected override void OnDestroy()
    {
        base.OnDestroy();
        m_Tips.Clear();
        m_Items.Clear();
    }

    private void LateUpdate()
    {
        foreach (var pair in m_Tips)
        {
            var info = pair.Value;
            var item = GetOrCreate(pair.Key);

            Vector3 worldPos = info.GetPos();

            // 可见性：相机存在 + 距离 + 遮挡 + 不在相机背后
            bool visible = m_Cam != null;
            Vector3 screenPos = default;
            bool behind = false;
            if (visible)
            {
                screenPos = m_Cam.WorldToScreenPoint(worldPos);
                behind = screenPos.z < 0f;
                if (behind && !info.ClampToScreenEdge)
                {
                    visible = false;
                }
            }

            var haveDuration = info.Duration >= 0;
            UIUtils.SetActive(item,haveDuration && visible);
            if(haveDuration) info.Duration -= Time.deltaTime;
            if (!visible) continue;

            item.SetContent(info);

            // 定位：背后镜像 + 可选边缘钳制
            if (behind)
            {
                screenPos.x = Screen.width - screenPos.x;
                screenPos.y = Screen.height - screenPos.y;
            }
            if (info.ClampToScreenEdge)
            {
                screenPos.x = Mathf.Clamp(screenPos.x, m_EdgeMargin, Screen.width - m_EdgeMargin);
                screenPos.y = Mathf.Clamp(screenPos.y, m_EdgeMargin, Screen.height - m_EdgeMargin);
            }
            var uiCam = m_Canvas != null && m_Canvas.renderMode != RenderMode.ScreenSpaceOverlay ? m_Canvas.worldCamera : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_TipsRoot, screenPos, uiCam, out var local))
            {
                item.Rect.anchoredPosition = local;
            }
        }
    }

    private void HandleLandingSurfaceTips()
    {
        
    }

    private TipsItemCtrl GetOrCreate(int tipId)
    {
        if (m_Items.TryGetValue(tipId, out var item) && item != null)
        {
            return item;
        }
        item = Instantiate(m_ItemTemplate, m_TipsRoot);
        m_Items[tipId] = item;
        return item;
    }
}