using System.Collections.Generic;
using UnityEngine;

namespace FurryChaos.Networking
{
    public sealed class SnapshotBuffer
    {
        private readonly List<NetworkCharacterSnapshot> m_Snapshots;
        private readonly int m_Capacity;

        public SnapshotBuffer(int capacity = 32)
        {
            m_Capacity = Mathf.Max(2, capacity);
            m_Snapshots = new List<NetworkCharacterSnapshot>(m_Capacity);
        }

        public void Add(in NetworkCharacterSnapshot snapshot)
        {
            if (m_Snapshots.Count > 0 &&
                snapshot.ServerTick <= m_Snapshots[m_Snapshots.Count - 1].ServerTick)
            {
                return;
            }

            m_Snapshots.Add(snapshot);
            if (m_Snapshots.Count > m_Capacity)
                m_Snapshots.RemoveAt(0);
        }

        public bool TrySample(double renderTick, out NetworkCharacterSnapshot sample)
        {
            sample = default;
            if (m_Snapshots.Count == 0)
                return false;

            while (m_Snapshots.Count >= 2 && m_Snapshots[1].ServerTick <= renderTick)
                m_Snapshots.RemoveAt(0);

            if (m_Snapshots.Count == 1)
            {
                sample = m_Snapshots[0];
                return true;
            }

            var from = m_Snapshots[0];
            var to = m_Snapshots[1];
            var tickRange = to.ServerTick - from.ServerTick;
            var t = tickRange == 0
                ? 1f
                : Mathf.Clamp01((float)((renderTick - from.ServerTick) / tickRange));

            sample = to;
            sample.Position = Vector3.Lerp(from.Position, to.Position, t);
            sample.Rotation = Quaternion.Slerp(from.Rotation, to.Rotation, t);
            sample.Velocity = Vector3.Lerp(from.Velocity, to.Velocity, t);
            sample.JumpNormalizedTime =
                Mathf.Lerp(from.JumpNormalizedTime, to.JumpNormalizedTime, t);
            return true;
        }

        public void Clear()
        {
            m_Snapshots.Clear();
        }
    }
}
