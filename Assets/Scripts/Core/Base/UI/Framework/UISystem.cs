using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class UISystem : LogicSystem
{
    private struct StackItem
    {
        public UIPanel Panel;
        public IUIArgs Args;
    }

    private readonly List<StackItem> m_Stack = new();
    private GameUIRoot m_Root;

    public UIRegister UIRegister { get; } = new();
    public int StackCount => m_Stack.Count;
    public UIPanel Top => m_Stack.Count > 0 ? m_Stack[m_Stack.Count - 1].Panel : null;

    public override void OnInit()
    {
        base.OnInit();
#if !UNITY_SERVER
        BindRoot(GameUIRoot.EnsureInstance());
#endif
    }

    public void BindRoot(GameUIRoot root)
    {
        m_Root = root;
        if (m_Root == null)
        {
            Log.Error("UISystem 绑定 GameUIRoot 失败");
            return;
        }

        m_Root.BindMaskClick(OnMaskClicked);
        m_Root.RegisterPrefabs(UIRegister);
        m_Root.HideMask();
    }

    public override void OnAfterAllSystemInit()
    {
        base.OnAfterAllSystemInit();
#if !UNITY_SERVER
        m_Root?.OpenDefaultPanel();
#endif
    }

    public T Open<T>(IUIArgs args = null) where T : UIPanel
    {
        var prefab = UIRegister.GetPrefab<T>();
        if (prefab == null)
        {
            Log.Error($"UIRegister 中未注册 {typeof(T).Name}");
            return null;
        }

        return Open(prefab, args) as T;
    }

    public UIPanel Open(UIPanel prefab, IUIArgs args = null)
    {
        if (m_Root == null)
        {
            Log.Error("UISystem 尚未绑定 GameUIRoot");
            return null;
        }

        if (prefab == null)
        {
            Log.Error("打开界面失败：预制体为空");
            return null;
        }

        var type = prefab.GetType();
        if (Top != null && type.IsInstanceOfType(Top))
        {
            Top.HandleOpen(args ?? UIEmptyArgs.Default);
            return Top;
        }

        var existing = Get(type);
        if (existing != null)
        {
            Log.Warning($"界面 {type.Name} 已在栈中，忽略重复打开");
            return existing;
        }

        // UIRegister.Register(prefab);

        if (m_Stack.Count > 0)
        {
            if (prefab.DisplayMode == UIDisplayMode.FullScreen)
            {
                for (int i = 0; i < m_Stack.Count; i++)
                {
                    m_Stack[i].Panel.HandlePause(true);
                }
            }
            else
            {
                Top.HandlePause(false);
            }
        }

        var parent = prefab.DisplayMode == UIDisplayMode.FullScreen
            ? m_Root.FullScreenRoot
            : m_Root.PopupRoot;

        var panel = Object.Instantiate(prefab, parent, false);
        panel.transform.SetAsLastSibling();
        m_Stack.Add(new StackItem
        {
            Panel = panel,
            Args = args
        });
        RefreshMask();
        panel.HandleOpen(args ?? UIEmptyArgs.Default);
        return panel;
    }

    public void Close()
    {
        CloseInternal(Top);
    }

    public void Close(UIPanel panel)
    {
        CloseInternal(panel);
    }

    public void Close<T>() where T : UIPanel
    {
        if (Top is T)
        {
            Close();
        }
    }

    public void CloseTo<T>() where T : UIPanel
    {
        if (!IsOpen<T>())
        {
            return;
        }

        while (m_Stack.Count > 0 && Top is not T)
        {
            Close();
        }
    }

    public void CloseAll()
    {
        while (m_Stack.Count > 0)
        {
            Close();
        }
    }

    public bool IsOpen<T>() where T : UIPanel
    {
        for (int i = 0; i < m_Stack.Count; i++)
        {
            if (m_Stack[i].Panel is T)
            {
                return true;
            }
        }

        return false;
    }

    public T Get<T>() where T : UIPanel
    {
        return Get(typeof(T)) as T;
    }

    private UIPanel Get(Type type)
    {
        if (type == null)
        {
            return null;
        }

        for (int i = m_Stack.Count - 1; i >= 0; i--)
        {
            if (type.IsInstanceOfType(m_Stack[i].Panel))
            {
                return m_Stack[i].Panel;
            }
        }

        return null;
    }

    public override void OnUpdate(float deltaTime)
    {
        if (Top != null && Top.AllowEscapeClose && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public override void OnDispose()
    {
        CloseAll();
        UIRegister.Clear();
        m_Root = null;
        base.OnDispose();
    }

    private void CloseInternal(UIPanel panel)
    {
        if (panel == null || m_Stack.Count == 0)
        {
            return;
        }

        if (Top != panel)
        {
            Log.Warning($"只能关闭栈顶界面，当前栈顶是 {Top.GetType().Name}");
            return;
        }

        m_Stack.RemoveAt(m_Stack.Count - 1);
        panel.HandleClose();
        Object.Destroy(panel.gameObject);
        RefreshMask();

        if (m_Stack.Count > 0)
        {
            Top.HandleResume();
        }
    }

    private void RefreshMask()
    {
        if (m_Root == null)
        {
            return;
        }

        if (Top != null && Top.DisplayMode == UIDisplayMode.Popup)
        {
            m_Root.ShowMask(Top.CloseOnClickMask);
            Top.transform.SetAsLastSibling();
            return;
        }

        m_Root.HideMask();
    }

    private void OnMaskClicked()
    {
        if (Top != null && Top.CloseOnClickMask)
        {
            Close();
        }
    }
}
