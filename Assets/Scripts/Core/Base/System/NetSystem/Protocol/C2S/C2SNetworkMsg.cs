public abstract class C2SNetworkMsg : NetworkMsg,IC2SMsg
{
    public override void Release()
    {
        NetworkMsg.Release(this);
    }
}