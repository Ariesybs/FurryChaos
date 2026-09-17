using System;

public static class GameTimer
{
    public static TimerHandle Register(float delay, Action callback, bool loop = false, bool useUnscaledTime = false)
    {
        var timer = GameRoot.Instance.GameTimer;
        if (timer != null)
        {
            return timer.Register(delay, callback, loop, useUnscaledTime);
        }

        return default;
    }

    public static TimerHandle RegisterOnce(float delay, Action callback, bool useUnscaledTime = false)
    {
        var timer = GameRoot.Instance.GameTimer;
        if (timer != null)
        {
            return timer.RegisterOnce(delay, callback, useUnscaledTime);
        }

        return default;
    }

    public static TimerHandle RegisterLoop(float interval, Action callback, bool useUnscaledTime = false)
    {
        var timer = GameRoot.Instance.GameTimer;
        if (timer != null)
        {
            return timer.RegisterLoop(interval, callback, useUnscaledTime);
        }

        return default;
    }

    public static bool Cancel(TimerHandle handle)
    {
        var timer = GameRoot.Instance.GameTimer;
        if (timer != null)
        {
            return timer.Cancel(handle);
        }

        return false;
    }
}