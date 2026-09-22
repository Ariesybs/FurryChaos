public class InGameSystem : SubSystem
{
    public InGameSystem(ISystem mainSystem) : base(mainSystem)
    {
    }

    public void SpawnPlayer(ulong clientId)
    {
        // 查找出生点
        // Instantiate 玩家预制体
        // SpawnAsPlayerObject(clientId, true)
    }
}