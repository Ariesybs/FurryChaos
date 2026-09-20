using Unity.Netcode;
using UnityEngine;

public struct CatSnapshot : INetworkSerializable
{
    public uint ServerTick;
    public uint LastProcessedInputSequence;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Velocity;
    public byte AnimationState;
    public bool IsGrounded;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref ServerTick);
        serializer.SerializeValue(ref LastProcessedInputSequence);
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref Rotation);
        serializer.SerializeValue(ref Velocity);
        serializer.SerializeValue(ref AnimationState);
        serializer.SerializeValue(ref IsGrounded);
    }
}

[RequireComponent(typeof(NetworkObject))]
public sealed class CatNetworkEntity : NetworkBehaviour
{
    public bool CanReadLocalInput => IsSpawned && IsOwner;
    public bool RunsAuthoritativeSimulation => IsSpawned && IsServer;
    public uint CurrentNetworkTick =>
        IsSpawned ? (uint)NetworkManager.LocalTime.Tick : 0;

    private CatCharacter m_Character;
    private uint m_LastProcessedInputSequence;
    private Vector3 m_NetworkVelocity;
    private CatCharacter.CatAnimationState m_AnimationState;
    private CatCharacter.CatAnimationState m_LastAnimationState;

    [Header("网络平滑")]
    [SerializeField, Min(0.1f)]
    private float m_PositionSharpness = 12f;
    [SerializeField, Min(0.1f)]
    private float m_RotationSharpness = 15f;
    [SerializeField, Min(0.1f)]
    private float m_TeleportDistance = 3f;
    private Vector3 m_TargetPosition;
    private Quaternion m_TargetRotation;
    private bool m_HasSnapshot;

    private void Awake()
    {
        m_Character = GetComponent<CatCharacter>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        ConfigureSimulation();

        if (IsServer)
        {
            NetworkManager.NetworkTickSystem.Tick += SendAuthoritativeSnapshot;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager != null && NetworkManager.NetworkTickSystem != null)
        {
            NetworkManager.NetworkTickSystem.Tick -= SendAuthoritativeSnapshot;
        }

        base.OnNetworkDespawn();
    }

    public void SubmitLocalInput(InputCmd command)
    {
        if (!CanReadLocalInput)
        {
            return;
        }

        if (IsServer)
        {
            ApplyAuthoritativeInput(command);
            return;
        }

        if (m_Character != null && m_Character.enableLocalPredict)
        {
            m_Character.ApplyNetworkInput(command);
        }

        SubmitInputServerRpc(command);
    }

    [ServerRpc(Delivery = RpcDelivery.Unreliable)]
    private void SubmitInputServerRpc(InputCmd command)
    {
        ApplyAuthoritativeInput(command);
    }

    private void ApplyAuthoritativeInput(InputCmd command)
    {
        if (!IsServer || m_Character == null)
        {
            return;
        }

        m_LastProcessedInputSequence = command.Sequence;
        m_Character.ApplyNetworkInput(command);
    }

    public void ApplySnapshot(Vector3 position, Quaternion rotation, Vector3 velocity, CatCharacter.CatAnimationState animationState)
    {
        if (m_Character?.motor == null)
        {
            return;
        }

        m_TargetPosition = position;
        m_TargetRotation = rotation;
        m_NetworkVelocity = velocity;
        m_AnimationState = animationState;
        if (!m_HasSnapshot)
        {
            m_HasSnapshot = true;
            // 第一帧直接同步，避免从场景默认位置缓慢移动过来。
            m_Character.motor.SetPositionAndRotation(position, rotation, true);
        }
    }

    private void Update()
    {
        UpdateNetworkTransform();
        if (!m_HasSnapshot ||
            (IsOwner &&
             m_Character != null &&
             m_Character.enableLocalPredict) ||
            m_Character?.animancer == null)
        {
            return;
        }
        if (m_AnimationState != m_LastAnimationState)
        {
            m_LastAnimationState = m_AnimationState;
            switch (m_AnimationState)
            {
                case CatCharacter.CatAnimationState.Jump:
                    m_Character.animancer.SwitchAnimation("Jump");
                    break;
                case CatCharacter.CatAnimationState.Idle:
                    m_Character.animancer.SwitchAnimation("Idle");
                    break;
            }
        }
        switch (m_AnimationState)
        {
            case CatCharacter.CatAnimationState.Crouch:
                m_Character.animancer.UpdateCrouchVelocity(m_NetworkVelocity, Time.deltaTime);
                break;
            case CatCharacter.CatAnimationState.Walk:
            case CatCharacter.CatAnimationState.Run:
                m_Character.animancer.UpdateLocomotionVelocity(m_NetworkVelocity, Time.deltaTime);
                break;
        }
    }

    private void UpdateNetworkTransform()
    {
        if (!m_HasSnapshot ||
            IsServer ||
            m_Character?.motor == null)
        {
            return;
        }
        Vector3 currentPosition = m_Character.motor.TransientPosition;
        Quaternion currentRotation = m_Character.motor.TransientRotation;
        float distanceSqr = (m_TargetPosition - currentPosition).sqrMagnitude;
        // 误差太大时直接传送，防止一直追不上服务器。
        if (distanceSqr >= m_TeleportDistance * m_TeleportDistance)
        {
            m_Character.motor.SetPositionAndRotation(m_TargetPosition, m_TargetRotation, true);
            return;
        }
        float positionT = 1f - Mathf.Exp(-m_PositionSharpness * Time.deltaTime);
        float rotationT = 1f - Mathf.Exp(-m_RotationSharpness * Time.deltaTime);
        Vector3 smoothPosition = Vector3.Lerp(currentPosition, m_TargetPosition, positionT);
        Quaternion smoothRotation = Quaternion.Slerp(currentRotation, m_TargetRotation, rotationT);
        m_Character.motor.SetPositionAndRotation(smoothPosition, smoothRotation, true);
    }

    private void ConfigureSimulation()
    {
        if (m_Character?.motor == null)
        {
            return;
        }

        // 房主模拟所有角色，普通客户端只预测自己拥有的角色。
        m_Character.motor.enabled = IsServer || (IsOwner && m_Character.enableLocalPredict);
    }

    private void SendAuthoritativeSnapshot()
    {
        if (!IsServer || m_Character?.motor == null)
        {
            return;
        }

        var snapshot = new CatSnapshot
        {
            ServerTick = (uint)NetworkManager.ServerTime.Tick,
            LastProcessedInputSequence = m_LastProcessedInputSequence,
            Position = m_Character.motor.TransientPosition,
            Rotation = m_Character.motor.TransientRotation,
            Velocity = m_Character.motor.Velocity,
            AnimationState = (byte)m_Character.CurrentAnimationState,
            IsGrounded = m_Character.motor.GroundingStatus.IsStableOnGround
        };

        ReceiveSnapshotClientRpc(snapshot);
    }

    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    private void ReceiveSnapshotClientRpc(CatSnapshot snapshot)
    {
        if (IsServer)
        {
            return;
        }

        ApplySnapshot(
            snapshot.Position,
            snapshot.Rotation,
            snapshot.Velocity,
            (CatCharacter.CatAnimationState)snapshot.AnimationState);
    }
}