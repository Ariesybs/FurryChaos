public class SdkSystem : LogicSystem
{
    public SteamSdk SteamSdk;

    public override void OnInit()
    {
        base.OnInit();
        SteamSdk = new SteamSdk();
        SteamSdk.OnInit();
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        SteamSdk.OnUpdate(deltaTime);
    }
}