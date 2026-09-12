public static class Log
{
    public static void Debug(string msg)
    {
        var log = GameRoot.Instance.GetSystem<LogSystem>();
        log?.Debug(msg);
    }
    
    public static void Info(string msg)
    {
        var log = GameRoot.Instance.GetSystem<LogSystem>();
        log?.Info(msg);
    }
    
    public static void Warning(string msg)
    {
        var log = GameRoot.Instance.GetSystem<LogSystem>();
        log?.Warning(msg);
    }
    
    public static void Error(string msg)
    {
        var log = GameRoot.Instance.GetSystem<LogSystem>();
        log?.Error(msg);
    }
    
    public static void Fatal(string msg)
    {
        var log = GameRoot.Instance.GetSystem<LogSystem>();
        log?.Fatal(msg);
    }
}