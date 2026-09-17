public static class GameNet
{
    public static long MyConnectionId => GetMyConnectionId();
    public static void SendS(NetworkMsg msg)
    {
        if (msg == null)
        {
            return;
        }

        var netSystem = GameRoot.Instance.GetSystem<NetworkSystem>();
        netSystem?.Send(msg);
    } 
    
    public static void SendC(long connectionId,NetworkMsg msg)
    {
        if (msg == null)
        {
            return;
        }

        var netSystem = GameRoot.Instance.GetSystem<NetworkSystem>();
        netSystem?.Send(connectionId,msg);
    } 
    
    public static void Broadcast(NetworkMsg msg)
    {
        if (msg == null)
        {
            return;
        }
        GameRoot.Instance.GameNet?.Broadcast(msg);
    }

    public static long GetMyConnectionId()
    {
        return 1;
        var netSystem = GameRoot.Instance.GetSystem<NetworkSystem>();
        return netSystem.ConnectionId;
    }
}