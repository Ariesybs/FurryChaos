public static class ClientMsgHandler
{
    public static void HandlePlayerJoin(S2C_JoinResponse msg)
    {
        Log.Info($"Set Connection Id {msg.ConnectionId}");
        GameRoot.Instance.GetSystem<NetworkSystem>().ConnectionId = msg.ConnectionId;
        GameSceneLoader.LoadScene(SceneDefine.GameScene);
    }
}