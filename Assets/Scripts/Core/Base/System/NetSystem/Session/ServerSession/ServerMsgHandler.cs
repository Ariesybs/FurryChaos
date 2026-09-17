public static class ServerMsgHandler
{
    public static void HandlePlayerJoin(int connectionId)
    {
        var msg = NetworkMsg.Get<S2C_JoinResponse>();
        msg.ConnectionId = connectionId;
        GameSceneLoader.LoadScene(SceneDefine.GameScene);
        GameNet.SendC(connectionId,msg);
        NetworkMsg.Release(msg);
    }

    public static void HandleCatMove(int entityId, C2S_CatInputRequest msg)
    {
        var syncSystem = GameRoot.Instance.GamePlayer.GameCatSyncSystem;
        if (syncSystem != null)
        {
            syncSystem.HandleClientInput(entityId,InputCmd.GetCmdFromClientMsg(msg));
        }
    }
}