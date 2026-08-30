using UnityEngine;

namespace FurryChaos.Networking
{
    [DisallowMultipleComponent]
    public sealed class NetworkEntityIdentity : MonoBehaviour
    {
        [SerializeField] private long entityId;
        [SerializeField] private NetworkRole role = NetworkRole.Standalone;

        public long EntityId => entityId;
        public NetworkRole Role => role;
        public bool IsLocalPlayer => role == NetworkRole.LocalPredicted;
        public bool IsRemoteProxy => role == NetworkRole.RemoteProxy;
        public bool IsServerAuthority => role == NetworkRole.ServerAuthority;

        public void Initialize(long id, NetworkRole networkRole)
        {
            entityId = id;
            role = networkRole;
        }
    }
}
