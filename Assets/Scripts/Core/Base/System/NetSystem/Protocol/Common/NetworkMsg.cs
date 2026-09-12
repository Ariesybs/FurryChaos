using System;

public abstract class NetworkMsg : INetworkMsg
{
    public abstract ushort MsgType { get; }
    public abstract byte[] Encode();
    public abstract void Decode(byte[] payload);
    public abstract void Reset();

    public static T Get<T>() where T : NetworkMsg , new()
    {
        return NetworkMsgPool.Get<T>();
    }

    public static void Release(NetworkMsg msg)
    {
        NetworkMsgPool.Release(msg);
    }
}