public static class ClientMsgHandler
{
    public static void HandlePlayerJoin(S2C_JoinResponse msg)
    {
        Log.Info($"Set Connection Id {msg.ConnectionId}");
        GameRoot.Instance.GetSystem<NetworkSystem>().ConnectionId = msg.ConnectionId;
        GameSceneLoader.LoadScene(SceneDefine.GameScene);
    }
    
    public static void HandleCatSnapshot(S2C_CatSnapshot msg)
    {
        if (msg == null || msg.EntityId <= 0)
        {
            return;
        }
        GameRoot.Instance.GamePlayer?.GameCatSyncSystem?.HandleSnapshot(msg);
    }
}