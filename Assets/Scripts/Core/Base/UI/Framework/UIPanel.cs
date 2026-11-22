using UnityEngine;

public abstract class UIPanel : MonoBehaviour
{
    [SerializeField] private bool m_CloseOnClickMask = true;
    [SerializeField] private bool m_EnableShowCursor = true;

    public abstract UIDisplayMode DisplayMode { get; }

    public virtual bool CloseOnClickMask =>
        DisplayMode == UIDisplayMode.Popup && m_CloseOnClickMask;

    public virtual bool AllowEscapeClose => DisplayMode == UIDisplayMode.Popup;

    public bool IsPaused { get; private set; }

    private CanvasGroup m_CanvasGroup;

    internal void HandleOpen(object args)
    {
        gameObject.SetActive(true);
        IsPaused = false;
        OnOpen(args);
    }

    internal void HandlePause(bool hide)
    {
        IsPaused = true;
        OnPause();
        if (hide)
        {
            gameObject.SetActive(false);
        }
    }

    internal void HandleResume()
    {
        gameObject.SetActive(true);
        IsPaused = false;
        OnResume();
    }

    internal void HandleClose()
    {
        OnClose();
    }

    protected virtual void OnOpen(object args)
    {
        OnOpen();
        if (m_EnableShowCursor)
        {
            GameCursor.Unlock();
        }
    }

    protected virtual void OnOpen()
    {
    }

    protected virtual void OnPause()
    {
    }

    protected virtual void OnResume()
    {
    }

    protected virtual void OnClose()
    {
        GameCursor.Lock();
    }

    protected void CloseSelf()
    {
        UIMgr.Close(this);
    }
}

public abstract class UIPanel<TArgs> : UIPanel where TArgs : IUIArgs
{
    protected TArgs Args { get; private set; }

    protected sealed override void OnOpen(object args)
    {
        if (args is not TArgs typed)
        {
            Log.Error($"界面 {GetType().Name} 入参类型错误，期望 {typeof(TArgs).Name}，实际 {args?.GetType().Name ?? "null"}");
            return;
        }

        Args = typed;
        OnOpen(typed);
    }

    protected abstract void OnOpen(TArgs args);
}
