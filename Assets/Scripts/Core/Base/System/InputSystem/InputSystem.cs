using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 输入中枢。全项目唯一读取物理输入的地方；对外只暴露语义动作。
/// 轮询型消费者用 GetDown/GetHeld/ReadMoveAxis，界面型消费者订阅 ActionDown。
/// 默认绑定写在代码里保证开箱可用；手柄按钮用 KeyCode.JoystickButtonN 表示，
/// 摇杆走 Input Manager 的 Horizontal/Vertical 默认轴（已同时映射 WASD 和左摇杆）。
/// </summary>
public class InputSystem : LogicSystem
{
    private readonly Dictionary<GameAction, List<KeyCode>> m_Bindings = new();

    /// <summary>动作按下事件。订阅者必须在关闭/销毁时退订，否则会持有已销毁对象。</summary>
    public event Action<GameAction> ActionDown;

    /// <summary>有拦截输入的面板打开时由 UISystem 置 false，游戏性输入整体静默。</summary>
    public bool GameplayEnabled = true;

    public override void OnInit()
    {
        base.OnInit();
        InitDefaultBindings();
    }

    public bool GetDown(GameAction action) => AnyKey(action, Input.GetKeyDown);

    public bool GetHeld(GameAction action) => AnyKey(action, Input.GetKey);

    /// <summary>移动轴：Input Manager 默认轴已同时映射键盘和手柄左摇杆。</summary>
    public Vector2 ReadMoveAxis()
    {
        return Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);
    }

    /// <summary>运行时覆盖绑定，供以后的改键界面调用；需要持久化时自行存 PlayerPrefs。</summary>
    public void OverrideBinding(GameAction action, List<KeyCode> keys)
    {
        m_Bindings[action] = keys ?? new List<KeyCode>();
    }

    public override void OnUpdate(float deltaTime)
    {
        if (ActionDown == null)
        {
            return;
        }

        foreach (var pair in m_Bindings)
        {
            if (AnyKey(pair.Key, Input.GetKeyDown))
            {
                ActionDown.Invoke(pair.Key);
            }
        }
    }

    private bool AnyKey(GameAction action, Func<KeyCode, bool> check)
    {
        if (!m_Bindings.TryGetValue(action, out var keys))
        {
            return false;
        }

        for (var i = 0; i < keys.Count; i++)
        {
            if (check(keys[i]))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 默认绑定：键盘 + Xbox 布局手柄（A=0 B=1 X=2 Y=3，Start=7，左摇杆按下=8）。
    /// </summary>
    private void InitDefaultBindings()
    {
        m_Bindings[GameAction.Interact] = new List<KeyCode> { KeyCode.E, KeyCode.JoystickButton2 };
        m_Bindings[GameAction.Jump] = new List<KeyCode> { KeyCode.Space, KeyCode.JoystickButton0 };
        m_Bindings[GameAction.Sit] = new List<KeyCode> { KeyCode.C };
        m_Bindings[GameAction.Lie] = new List<KeyCode> { KeyCode.Z };
        m_Bindings[GameAction.Crouch] = new List<KeyCode> { KeyCode.LeftControl, KeyCode.JoystickButton1 };
        m_Bindings[GameAction.Run] = new List<KeyCode> { KeyCode.LeftShift, KeyCode.JoystickButton8 };
        m_Bindings[GameAction.Pause] = new List<KeyCode> { KeyCode.Escape, KeyCode.JoystickButton7 };
        m_Bindings[GameAction.Confirm] = new List<KeyCode> { KeyCode.Return, KeyCode.JoystickButton0 };
        m_Bindings[GameAction.Cancel] = new List<KeyCode> { KeyCode.Escape, KeyCode.JoystickButton1 };
        m_Bindings[GameAction.DebugPanel] = new List<KeyCode> { KeyCode.L };
    }
}
