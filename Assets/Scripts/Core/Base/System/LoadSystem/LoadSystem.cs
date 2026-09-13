public class LoadSystem : LogicSystem
{
    public SceneLoader GameSceneLoader;

    public override void OnInit()
    {
        base.OnInit();
        GameSceneLoader = new SceneLoader();
        GameSceneLoader.OnInit();
    }

    public override void OnAfterAllSystemInit()
    {
        base.OnAfterAllSystemInit();
        GameSceneLoader.OnAfterAllSystemInit();
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        GameSceneLoader.OnUpdate(deltaTime);
    }

    public override void OnDispose()
    {
        base.OnDispose();
        GameSceneLoader.OnDispose();
    }
}