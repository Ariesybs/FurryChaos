public interface IClientTransport : INetworkTransport
{
    bool Connect(string address, ushort port);
    void SendToServer(byte[] payload);
}