/// <summary>
/// S->C 客户端网络消息分发层
/// </summary>
public static class ClientMsgRouter
{
    public static void HandleNetworkMsg(ushort msgType, byte[] payload)
    {
        var type = (NetworkMessageType.S2C)msgType;
        Log.Info($"Receive Server Response Type {type}");
        switch (type)
        {
            case NetworkMessageType.S2C.JoinResponse:
                var msg = NetworkMsg.Get<S2C_JoinResponse>();
                msg.Decode(payload);
                ClientMsgHandler.HandlePlayerJoin(msg);
                NetworkMsg.Release(msg);
                break;
        }
    }
}