using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUIRoot : MonoBehaviour
{
    public static GameUIRoot Instance { get; private set; }

    [SerializeField] private RectTransform m_FullScreenRoot;
    [SerializeField] private RectTransform m_PopupRoot;
    [Header("遮罩相关")]
    [SerializeField] private CanvasGroup m_Mask;
    [SerializeField] private Button m_MaskButton;
    [Header("Default UI")]
    [SerializeField] private UIPanel m_DefaultPanel;
    [SerializeField] private bool m_OpenDefaultOnStart = true;

    [Header("UI Register")]
    [SerializeField] private List<UIPanel> m_RegisterPrefabs = new();

    public Transform FullScreenRoot => m_FullScreenRoot;
    public Transform PopupRoot => m_PopupRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureLayers();
        UIUtils.SetActive(m_Mask,false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static GameUIRoot EnsureInstance()
    {
        if (Instance != null)
        {
            Instance.EnsureLayers();
            return Instance;
        }

        var existing = FindFirstObjectByType<GameUIRoot>();
        if (existing != null)
        {
            existing.EnsureLayers();
            return existing;
        }

        var canvasGo = GameObject.Find("GameUI");
        if (canvasGo != null)
        {
            var root = canvasGo.GetComponent<GameUIRoot>();
            if (root == null)
            {
                root = canvasGo.AddComponent<GameUIRoot>();
            }
            root.EnsureLayers();
            return root;
        }

        return CreateCanvasRoot();
    }

    public void RegisterPrefabs(UIRegister register)
    {
        if (register == null)
        {
            return;
        }

        if (m_DefaultPanel != null)
        {
            register.Register(m_DefaultPanel);
        }

        if (m_RegisterPrefabs == null)
        {
            return;
        }

        for (int i = 0; i < m_RegisterPrefabs.Count; i++)
        {
            register.Register(m_RegisterPrefabs[i]);
        }
    }

    public void OpenDefaultPanel()
    {
        if (!m_OpenDefaultOnStart || m_DefaultPanel == null)
        {
            return;
        }

        var ui = GameRoot.Instance?.GameUI;
        if (ui == null)
        {
            Log.Error("打开默认界面失败：UISystem 未就绪");
            return;
        }

        ui.Open(m_DefaultPanel);
    }

    public void BindMaskClick(UnityEngine.Events.UnityAction onClick)
    {
        if (m_MaskButton == null)
        {
            return;
        }

        m_MaskButton.onClick.RemoveAllListeners();
        if (onClick != null)
        {
            UIUtils.ButtonBindListener(m_MaskButton, onClick);
        }
    }

    public void ShowMask(bool clickable)
    {
        if (m_Mask == null)
        {
            return;
        }

        UIUtils.SetActive(m_Mask,true);
        m_Mask.transform.SetParent(m_PopupRoot, false);
        Stretch(m_Mask.transform as RectTransform);
        m_Mask.transform.SetAsLastSibling();
        if (m_MaskButton != null)
        {
            m_MaskButton.interactable = clickable;
        }

        Tween.Alpha(m_Mask, 1, 0.25f);
    }

    public void HideMask()
    {
        if (m_Mask == null)
        {
            return;
        }

        Tween.Alpha(m_Mask, 0, 0.25f).OnComplete(() => UIUtils.SetActive(m_Mask,false));
    }

    public void EnsureLayers()
    {
        EnsureCanvas();
        EnsureEventSystem();

        if (m_FullScreenRoot == null)
        {
            m_FullScreenRoot = CreateLayer("FullScreenRoot");
        }

        if (m_PopupRoot == null)
        {
            m_PopupRoot = CreateLayer("PopupRoot");
        }
        

        m_FullScreenRoot.SetAsLastSibling();
        m_PopupRoot.SetAsLastSibling();
        HideMask();
    }

    private static GameUIRoot CreateCanvasRoot()
    {
        var go = new GameObject("GameUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        var root = go.AddComponent<GameUIRoot>();
        root.EnsureLayers();
        return root;
    }

    private void EnsureCanvas()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
        }

        if (GetComponent<CanvasScaler>() == null)
        {
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
        }

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        DontDestroyOnLoad(go);
    }

    private RectTransform CreateLayer(string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(transform, false);
        Stretch(rect);
        return rect;
    }
    

    private static void Stretch(RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localPosition = Vector3.zero;
    }
}
