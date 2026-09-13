public static class GameNet
{
    public static void Send(NetworkMsg msg)
    {
        if (msg == null)
        {
            return;
        }

        var netSystem = GameRoot.Instance.GetSystem<NetworkSystem>();
        netSystem?.Send(msg);
    } 
    
    public static void Send(int connectionId,NetworkMsg msg)
    {
        if (msg == null)
        {
            return;
        }

        var netSystem = GameRoot.Instance.GetSystem<NetworkSystem>();
        netSystem?.Send(connectionId,msg);
    } 
}