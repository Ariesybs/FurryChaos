using System;

namespace FurryChaos.Networking.Transports
{
    /// <summary>
    /// Fantasy 接入点。安装 Fantasy.Unity 后在此处将 Session 收发转换为通用传输事件。
    /// 游戏逻辑只依赖 INetworkTransport，不应直接依赖 Fantasy API。
    /// </summary>
    public sealed class FantasyTransportAdapter : INetworkTransport
    {
        public bool IsRunning => false;
        public bool IsServer => false;

#pragma warning disable CS0067
        public event Action<int> Connected;
        public event Action<int> Disconnected;
        public event Action<int, byte[]> DataReceived;
#pragma warning restore CS0067

        public bool StartServer(ushort port)
            => throw new NotSupportedException("Fantasy 适配器尚未实现。");

        public bool StartClient(string address, ushort port)
            => throw new NotSupportedException("Fantasy 适配器尚未实现。");

        public void Poll()
        {
        }

        public void Send(int connectionId, byte[] payload)
            => throw new NotSupportedException("Fantasy 适配器尚未实现。");

        public void Broadcast(byte[] payload)
            => throw new NotSupportedException("Fantasy 适配器尚未实现。");

        public void Stop()
        {
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
