using UnityEngine;

namespace FurryChaos.Networking
{
    [DisallowMultipleComponent]
    public sealed class LocalNetworkInputSender : MonoBehaviour
    {
        [SerializeField] private NetworkEntityIdentity identity;

        private uint m_Sequence;

        public void Submit(Vector2 direction, float cameraYaw, byte pressedActions, byte heldActions)
        {
            var network = GameNetworkManager.Instance;
            if (network == null || !network.IsRunning || network.IsServer)
                return;

            if (identity != null && !identity.IsLocalPlayer)
                return;

            var input = new NetworkInputFrame
            {
                EntityId = network.LocalEntityId,
                Sequence = ++m_Sequence,
                ClientTick = (uint)network.EstimatedServerTick,
                Direction = Vector2.ClampMagnitude(direction, 1f),
                CameraYaw = cameraYaw,
                PressedActions = pressedActions,
                HeldActions = heldActions
            };

            network.SendInput(input);
        }

        public void ResetSequence()
        {
            m_Sequence = 0;
        }
    }
}
