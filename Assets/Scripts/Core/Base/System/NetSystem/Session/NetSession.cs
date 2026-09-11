using System;

public abstract class NetSession : IDisposable
{
    public abstract void Init();
    public abstract void Poll();
    public abstract void Dispose();
}