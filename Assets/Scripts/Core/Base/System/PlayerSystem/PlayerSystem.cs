public class PlayerSystem : LogicSystem
{
    public CatSyncSystem GameCatSyncSystem;

    public override void OnInit()
    {
        base.OnInit();
        GameCatSyncSystem = new CatSyncSystem();
        GameCatSyncSystem.OnInit();
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        GameCatSyncSystem.OnUpdate(deltaTime);
    }

    public override void OnDispose()
    {
        base.OnDispose();
        GameCatSyncSystem.OnDispose();
    }
}