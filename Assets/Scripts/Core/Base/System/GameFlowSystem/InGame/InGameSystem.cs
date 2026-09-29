using System.Collections.Generic;

public class InGameSystem : SubSystem
{
    public InGameSession Session;
    public InGameSystem(ISystem mainSystem) : base(mainSystem)
    {
        
    }

    public void RegisterSession(InGameSession session)
    {
        Session = session;
    }

    public void SpawnPlayer(ulong clientId)
    {
        // 查找出生点
        // Instantiate 玩家预制体
        // SpawnAsPlayerObject(clientId, true)
    }
}