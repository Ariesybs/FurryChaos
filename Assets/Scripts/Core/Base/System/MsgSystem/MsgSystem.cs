using System;
using System.Collections.Generic;

/// <summary>
/// 强类型消息系统。默认仅在 Unity 主线程使用。
/// </summary>
public sealed class MsgSystem : LogicSystem
{
    private readonly Dictionary<Type, Delegate> m_Handlers = new();

    public void Register<T>(Action<T> handler) where T : struct, IMsg
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        var messageType = typeof(T);

        if (!m_Handlers.TryGetValue(messageType, out var existing))
        {
            m_Handlers.Add(messageType, handler);
            return;
        }

        var callbacks = (Action<T>)existing;

        // 防止重复订阅。
        foreach (var callback in callbacks.GetInvocationList())
        {
            if (callback.Equals(handler))
                return;
        }

        m_Handlers[messageType] = callbacks + handler;
    }

    public void Unregister<T>(Action<T> handler) where T : struct, IMsg
    {
        if (handler == null)
            return;

        var messageType = typeof(T);

        if (!m_Handlers.TryGetValue(messageType, out var existing))
            return;

        var callbacks = (Action<T>)existing - handler;

        if (callbacks == null)
            m_Handlers.Remove(messageType);
        else
            m_Handlers[messageType] = callbacks;
    }

    public void Trigger<T>(T message) where T : struct, IMsg
    {
        if (!m_Handlers.TryGetValue(typeof(T), out var existing))
            return;

        // 保存当前委托快照，允许回调期间订阅或取消订阅。
        var callbacks = (Action<T>)existing;
        callbacks.Invoke(message);
    }

    public void Clear()
    {
        m_Handlers.Clear();
    }

    public override void OnDispose()
    {
        Clear();
    }
}