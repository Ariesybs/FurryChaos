public static class UIMgr
{
    private static UISystem Sys => GameRoot.Instance?.GameUI;

    public static void Register<T>(T prefab) where T : UIPanel
    {
        Sys?.UIRegister?.Register(prefab);
    }

    public static T Open<T>(IUIArgs args = null) where T : UIPanel
    {
        return Sys?.Open<T>(args);
    }

    public static UIPanel Open(UIPanel prefab, IUIArgs args = null)
    {
        return Sys?.Open(prefab, args);
    }

    public static T Open<T, TArgs>(TArgs args) where T : UIPanel<TArgs> where TArgs : IUIArgs
    {
        return Sys?.Open<T>(args);
    }

    public static void Close()
    {
        Sys?.Close();
    }

    public static void Close(UIPanel panel)
    {
        Sys?.Close(panel);
    }

    public static void Close<T>() where T : UIPanel
    {
        Sys?.Close<T>();
    }

    public static void CloseTo<T>() where T : UIPanel
    {
        Sys?.CloseTo<T>();
    }

    public static void CloseAll()
    {
        Sys?.CloseAll();
    }

    public static bool IsOpen<T>() where T : UIPanel
    {
        return Sys != null && Sys.IsOpen<T>();
    }

    public static T Get<T>() where T : UIPanel
    {
        return Sys?.Get<T>();
    }
}
