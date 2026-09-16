public abstract class S2CNetworkMsg : NetworkMsg, IS2CMsg
{
    public override void Release()
    {
        NetworkMsg.Release(this);
    }
}