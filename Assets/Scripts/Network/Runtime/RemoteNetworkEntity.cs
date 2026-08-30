using System;
using UnityEngine;

namespace FurryChaos.Networking
{
    [DisallowMultipleComponent]
    public sealed class RemoteNetworkEntity : MonoBehaviour
    {
        [SerializeField] private NetworkEntityIdentity identity;
        [SerializeField, Min(0)] private int interpolationDelayTicks = 3;

        private readonly SnapshotBuffer m_SnapshotBuffer = new();
        private GameNetworkManager m_Network;

        public event Action<NetworkCharacterSnapshot> StateSampled;

        private void OnEnable()
        {
            m_Network = GameNetworkManager.Instance;
            if (m_Network != null)
                m_Network.SnapshotReceived += OnSnapshotReceived;
        }

        private void OnDisable()
        {
            if (m_Network != null)
                m_Network.SnapshotReceived -= OnSnapshotReceived;

            m_SnapshotBuffer.Clear();
            m_Network = null;
        }

        private void Update()
        {
            if (identity == null || !identity.IsRemoteProxy || m_Network == null)
                return;

            var renderTick = m_Network.EstimatedServerTick - interpolationDelayTicks;
            if (!m_SnapshotBuffer.TrySample(renderTick, out var snapshot))
                return;

            transform.SetPositionAndRotation(snapshot.Position, snapshot.Rotation);
            StateSampled?.Invoke(snapshot);
        }

        private void OnSnapshotReceived(NetworkCharacterSnapshot snapshot)
        {
            if (identity != null && snapshot.EntityId == identity.EntityId)
                m_SnapshotBuffer.Add(snapshot);
        }
    }
}
