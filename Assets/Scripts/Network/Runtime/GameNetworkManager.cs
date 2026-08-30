using System;
using FurryChaos.Networking.Transports;
using UnityEngine;

namespace FurryChaos.Networking
{
    [DisallowMultipleComponent]
    public sealed class GameNetworkManager : MonoBehaviour
    {
        [Header("启动方式")]
        [SerializeField] private NetworkLaunchMode launchMode = NetworkLaunchMode.Disabled;
        [SerializeField] private bool autoStart = true;

        [Header("连接")]
        [SerializeField] private string serverAddress = "127.0.0.1";
        [SerializeField] private ushort serverPort = 7777;
        [SerializeField, Min(1)] private int serverTickRate = 30;

        private INetworkTransport m_Transport;
        private SingleRoomServer m_ServerRoom;
        private long m_LocalEntityId;
        private uint m_LatestServerTick;
        private double m_LatestServerTickReceiveTime;

        public static GameNetworkManager Instance { get; private set; }

        public bool IsRunning => m_Transport?.IsRunning == true;
        public bool IsServer => m_Transport?.IsServer == true;
        public long LocalEntityId => m_LocalEntityId;
        public int ServerTickRate => serverTickRate;
        public double EstimatedServerTick =>
            m_LatestServerTick +
            (Time.unscaledTimeAsDouble - m_LatestServerTickReceiveTime) * serverTickRate;

        public event Action Connected;
        public event Action Disconnected;
        public event Action<long> LocalPlayerAssigned;
        public event Action<long> ServerPlayerJoined;
        public event Action<long> ServerPlayerLeft;
        public event Action<int, NetworkInputFrame> ServerInputReceived;
        public event Action<NetworkCharacterSnapshot> SnapshotReceived;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
#if UNITY_SERVER
            launchMode = NetworkLaunchMode.Server;
#endif
            if (autoStart && launchMode != NetworkLaunchMode.Disabled)
                StartNetwork();
        }

        private void Update()
        {
            m_Transport?.Poll();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            StopNetwork();
        }

        public bool StartNetwork()
        {
            if (IsRunning)
                return true;

            m_Transport = new UnityTransportAdapter();
            m_Transport.Connected += OnTransportConnected;
            m_Transport.Disconnected += OnTransportDisconnected;

            var started = launchMode switch
            {
                NetworkLaunchMode.Server => StartServer(),
                NetworkLaunchMode.Client => StartClient(),
                _ => false
            };

            if (!started)
            {
                Debug.LogError(
                    $"网络启动失败：模式={launchMode}, 地址={serverAddress}, 端口={serverPort}");
                StopNetwork();
            }

            return started;
        }

        public void StopNetwork()
        {
            if (m_ServerRoom != null)
            {
                m_ServerRoom.PlayerJoined -= OnServerPlayerJoined;
                m_ServerRoom.PlayerLeft -= OnServerPlayerLeft;
                m_ServerRoom.InputReceived -= OnServerInputReceived;
                m_ServerRoom.Dispose();
                m_ServerRoom = null;
            }

            if (m_Transport != null)
            {
                m_Transport.Connected -= OnTransportConnected;
                m_Transport.Disconnected -= OnTransportDisconnected;
                m_Transport.DataReceived -= OnClientDataReceived;
                m_Transport.Stop();
                m_Transport.Dispose();
                m_Transport = null;
            }

            m_LocalEntityId = 0;
        }

        public void SendInput(in NetworkInputFrame input)
        {
            if (!IsRunning || IsServer)
                return;

            var ownedInput = input;
            ownedInput.EntityId = m_LocalEntityId;
            m_Transport.Send(0, NetworkPacketCodec.EncodeInputFrame(ownedInput));
        }

        public void BroadcastSnapshot(in NetworkCharacterSnapshot snapshot)
        {
            if (!IsServer)
                return;

            m_ServerRoom?.BroadcastSnapshot(snapshot);
        }

        private bool StartServer()
        {
            if (!m_Transport.StartServer(serverPort))
                return false;

            m_ServerRoom = new SingleRoomServer(m_Transport);
            m_ServerRoom.PlayerJoined += OnServerPlayerJoined;
            m_ServerRoom.PlayerLeft += OnServerPlayerLeft;
            m_ServerRoom.InputReceived += OnServerInputReceived;
            Debug.Log($"单房间服务器已启动，端口：{serverPort}");
            return true;
        }

        private bool StartClient()
        {
            m_Transport.DataReceived += OnClientDataReceived;
            Debug.Log($"正在连接服务器：{serverAddress}:{serverPort}");
            return m_Transport.StartClient(serverAddress, serverPort);
        }

        private void OnTransportConnected(int connectionId)
        {
            if (IsServer)
                return;

            m_Transport.Send(0, NetworkPacketCodec.EncodeJoinRequest());
            Connected?.Invoke();
        }

        private void OnTransportDisconnected(int connectionId)
        {
            if (!IsServer)
            {
                m_LocalEntityId = 0;
                Disconnected?.Invoke();
            }
        }

        private void OnClientDataReceived(int connectionId, byte[] payload)
        {
            if (!NetworkPacketCodec.TryGetMessageType(payload, out var messageType))
                return;

            switch (messageType)
            {
                case NetworkMessageType.JoinAccepted:
                    if (NetworkPacketCodec.TryDecodeJoinAccepted(payload, out var entityId))
                    {
                        m_LocalEntityId = entityId;
                        LocalPlayerAssigned?.Invoke(entityId);
                    }
                    break;

                case NetworkMessageType.CharacterSnapshot:
                    if (NetworkPacketCodec.TryDecodeSnapshot(payload, out var snapshot))
                    {
                        m_LatestServerTick = Math.Max(m_LatestServerTick, snapshot.ServerTick);
                        m_LatestServerTickReceiveTime = Time.unscaledTimeAsDouble;
                        SnapshotReceived?.Invoke(snapshot);
                    }
                    break;
            }
        }

        private void OnServerPlayerJoined(long entityId)
            => ServerPlayerJoined?.Invoke(entityId);

        private void OnServerPlayerLeft(long entityId)
            => ServerPlayerLeft?.Invoke(entityId);

        private void OnServerInputReceived(int connectionId, NetworkInputFrame input)
            => ServerInputReceived?.Invoke(connectionId, input);
    }
}
