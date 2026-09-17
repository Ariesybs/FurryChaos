using System;

public interface INetworkTransport : IDisposable
{
    bool IsRunning { get; }

    event Action Connected;
    event Action Disconnected;
    event Action<byte[]> DataReceived;

    void Poll();
    void Stop();
}