using System;

public interface IServerTransport : IDisposable
{
    bool IsRunning { get; }

    event Action<int> ClientConnected;
    event Action<int> ClientDisconnected;
    event Action<int, byte[]> DataReceived;

    bool Listen(ushort port);
    void SendToClient(int connectionId, byte[] payload);
    void Broadcast(byte[] payload);
    void Poll();
    void Stop();
}