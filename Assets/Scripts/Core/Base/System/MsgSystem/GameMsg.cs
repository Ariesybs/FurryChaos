using System;

public static class GameMsg
{
    public static void Register<T>(Action<T> handler) where T : struct, IMsg
    {
        var msgSystem = GameRoot.Instance.GetSystem<MsgSystem>();
        msgSystem?.Register(handler);
    }

    public static void Unregister<T>(Action<T> handler) where T : struct, IMsg
    {
        var msgSystem = GameRoot.Instance.GetSystem<MsgSystem>();
        msgSystem?.Unregister(handler);
    }
    
    public static void Trigger<T>(T msg) where T : struct, IMsg
    {
        var msgSystem = GameRoot.Instance.GetSystem<MsgSystem>();
        msgSystem?.Trigger(msg);
    }
}