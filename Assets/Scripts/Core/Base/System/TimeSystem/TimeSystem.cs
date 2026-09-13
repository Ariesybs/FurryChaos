using System;
using System.Collections.Generic;
using UnityEngine;

public readonly struct TimerHandle : IEquatable<TimerHandle>
{
    internal readonly int Id;

    internal TimerHandle(int id)
    {
        Id = id;
    }

    public bool IsValid => Id > 0;

    public bool Equals(TimerHandle other) => Id == other.Id;
    public override bool Equals(object obj) => obj is TimerHandle other && Equals(other);

    public override int GetHashCode() => Id;

    public static bool operator ==(TimerHandle left, TimerHandle right) => left.Equals(right);

    public static bool operator !=(TimerHandle left, TimerHandle right) => !left.Equals(right);
}

public sealed class TimeSystem : LogicSystem
{
    private sealed class TimerTask
    {
        public int Id;
        public float Interval;
        public float Remaining;
        public bool Loop;
        public bool UseUnscaledTime;
        public bool Paused;
        public Action Callback;
    }

    private readonly Dictionary<int, TimerTask> m_Timers = new();
    private readonly List<int> m_UpdateBuffer = new();

    private int m_NextTimerId = 1;
    private bool m_IsUpdating;

    /// <summary>
    /// 注册定时器。
    /// </summary>
    /// <param name="delay">首次触发时间，单位为秒。</param>
    /// <param name="callback">定时器回调。</param>
    /// <param name="loop">是否循环触发。</param>
    /// <param name="useUnscaledTime">
    /// 是否忽略 Time.timeScale。网络定时器应设为 true。
    /// </param>
    public TimerHandle Register(float delay, Action callback, bool loop = false, bool useUnscaledTime = false)
    {
        if (callback == null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        if (delay < 0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delay),
                "定时器延迟不能小于零。");
        }

        if (loop && delay <= 0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delay),
                "循环定时器间隔必须大于零。");
        }

        int id = AllocateTimerId();

        m_Timers.Add(id, new TimerTask
        {
            Id = id,
            Interval = delay,
            Remaining = delay,
            Loop = loop,
            UseUnscaledTime = useUnscaledTime,
            Callback = callback
        });

        return new TimerHandle(id);
    }

    /// <summary>
    /// 注册只执行一次的定时器。
    /// </summary>
    public TimerHandle RegisterOnce(float delay, Action callback, bool useUnscaledTime = false)
    {
        return Register(delay, callback, false, useUnscaledTime);
    }

    /// <summary>
    /// 注册循环定时器。
    /// </summary>
    public TimerHandle RegisterLoop(float interval, Action callback, bool useUnscaledTime = false)
    {
        return Register(interval, callback, true, useUnscaledTime);
    }

    public bool Cancel(TimerHandle handle)
    {
        return handle.IsValid && m_Timers.Remove(handle.Id);
    }

    public bool Pause(TimerHandle handle)
    {
        if (!TryGetTask(handle, out TimerTask task))
        {
            return false;
        }

        task.Paused = true;
        return true;
    }

    public bool Resume(TimerHandle handle)
    {
        if (!TryGetTask(handle, out TimerTask task))
        {
            return false;
        }

        task.Paused = false;
        return true;
    }

    /// <summary>
    /// 从初始间隔重新开始计时。
    /// </summary>
    public bool Restart(TimerHandle handle)
    {
        if (!TryGetTask(handle, out TimerTask task))
        {
            return false;
        }

        task.Remaining = task.Interval;
        task.Paused = false;
        return true;
    }

    public bool Contains(TimerHandle handle)
    {
        return handle.IsValid && m_Timers.ContainsKey(handle.Id);
    }

    public bool TryGetRemaining(TimerHandle handle, out float remaining)
    {
        if (TryGetTask(handle, out TimerTask task))
        {
            remaining = Mathf.Max(0f, task.Remaining);
            return true;
        }

        remaining = 0f;
        return false;
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);

        // 防止回调中意外递归更新。
        if (m_IsUpdating || m_Timers.Count == 0)
        {
            return;
        }

        m_IsUpdating = true;
        m_UpdateBuffer.Clear();

        // 使用快照，允许回调安全地注册或取消定时器。
        foreach (int id in m_Timers.Keys)
        {
            m_UpdateBuffer.Add(id);
        }

        var scaledDelta = Mathf.Max(0f, deltaTime);
        var unscaledDelta = Mathf.Max(0f, Time.unscaledDeltaTime);

        try
        {
            for (int i = 0; i < m_UpdateBuffer.Count; i++)
            {
                int id = m_UpdateBuffer[i];

                if (!m_Timers.TryGetValue(id, out TimerTask task) ||
                    task.Paused)
                {
                    continue;
                }

                task.Remaining -= task.UseUnscaledTime ? unscaledDelta : scaledDelta;

                if (task.Remaining > 0f)
                {
                    continue;
                }

                if (task.Loop)
                {
                    // 不补发卡顿期间错过的回调，避免心跳瞬间连发。
                    task.Remaining = task.Interval;
                }
                else
                {
                    // 回调前删除，回调可以安全注册新的定时器。
                    m_Timers.Remove(id);
                }

                try
                {
                    task.Callback.Invoke();
                }
                catch (Exception exception)
                {
                    Log.Error($"定时器回调执行异常，TimerId={id}", exception);
                }
            }
        }
        finally
        {
            m_UpdateBuffer.Clear();
            m_IsUpdating = false;
        }
    }

    public override void OnDispose()
    {
        base.OnDispose();

        m_Timers.Clear();
        m_UpdateBuffer.Clear();
        m_NextTimerId = 1;
        m_IsUpdating = false;
    }

    private bool TryGetTask(TimerHandle handle, out TimerTask task)
    {
        if (!handle.IsValid)
        {
            task = null;
            return false;
        }

        return m_Timers.TryGetValue(handle.Id, out task);
    }

    private int AllocateTimerId()
    {
        while (m_NextTimerId <= 0 || m_Timers.ContainsKey(m_NextTimerId))
        {
            m_NextTimerId++;

            if (m_NextTimerId <= 0)
            {
                m_NextTimerId = 1;
            }
        }

        return m_NextTimerId++;
    }
}