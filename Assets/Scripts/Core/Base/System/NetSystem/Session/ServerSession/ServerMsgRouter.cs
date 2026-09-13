/// <summary>
/// C->S 服务器网络消息分发层
/// </summary>
public static class ServerMsgRouter
{
    public static void HandleNetworkMsg(int connectionId, ushort msgType)
    {
        var type = (NetworkMessageType.C2S)msgType;
        Log.Info($"Receive Client Request Type {type}");
        switch (type)
        {
            case NetworkMessageType.C2S.JoinRequest:
                ServerMsgHandler.HandlePlayerJoin(connectionId);
                break;
        }        
    }
}