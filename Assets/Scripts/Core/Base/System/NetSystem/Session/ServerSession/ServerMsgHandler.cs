public static class ServerMsgHandler
{
    public static void HandlePlayerJoin(int connectionId)
    {
        var msg = NetworkMsg.Get<S2C_JoinResponse>();
        msg.ConnectionId = connectionId;
        GameNet.Send(connectionId,msg);
        NetworkMsg.Release(msg);
    }
}