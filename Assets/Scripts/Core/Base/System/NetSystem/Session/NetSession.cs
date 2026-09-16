using System;

public abstract class NetSession : IDisposable
{
    public abstract void Init();
    public virtual void Send(NetworkMsg msg){}
    public virtual void Send(long connectionId,NetworkMsg msg) {}
    public virtual void Broadcast(NetworkMsg msg)
    {
    }
    public abstract void Poll();
    public abstract void Dispose();
}