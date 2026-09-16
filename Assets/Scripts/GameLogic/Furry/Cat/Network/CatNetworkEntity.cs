// EntityId、是否本地玩家

using System;
using UnityEngine;

public enum CatNetworkRole : byte
{
    LocalPlayer,
    ServerAuthority,
    RemoteProxy
}
public class CatNetworkEntity : MonoBehaviour
{
    [SerializeField]
    private long m_EntityId;
    [SerializeField]
    private CatNetworkRole m_Role = CatNetworkRole.LocalPlayer;
    public long EntityId => m_EntityId;
    public CatNetworkRole Role => m_Role;
    private CatCharacter Character;
    private CatSyncSystem m_SyncSystem;
    private bool m_Registered;
    
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
        Character = GetComponent<CatCharacter>();
        ApplyRole();
    }

    private void Start()
    {
        m_EntityId = GameNet.MyConnectionId;
        Register();
    }

    public void Setup(long entityId, CatNetworkRole role)
    {
        m_EntityId = entityId;
        m_Role = role;

        if (Character?.motor != null)
        {
            Character.motor.enabled = role != CatNetworkRole.RemoteProxy;
        }
    }
    
    private void Register()
    {
        if (m_Registered || GameRoot.Instance == null)
        {
            return;
        }
        var syncSystem = GameRoot.Instance.GamePlayer?.GameCatSyncSystem;
        if (syncSystem == null)
        {
            return;
        }
        syncSystem.RegisterCat(this);
        m_Registered = true;
    }

    // 服务器调用
    public void ApplyInput(InputCmd command)
    {
        if (m_Role != CatNetworkRole.ServerAuthority)
        {
            return;
        }
        Character.catFsm?.OnInput(command);
    }
    
    public void ApplySnapshot(Vector3 position, Quaternion rotation, Vector3 velocity, CatCharacter.CatAnimationState animationState)
    {
        m_TargetPosition = position;
        m_TargetRotation = rotation;
        m_NetworkVelocity = velocity;
        m_AnimationState = animationState;
        if (!m_HasSnapshot)
        {
            m_HasSnapshot = true;
            // 第一帧直接同步，避免从场景默认位置缓慢移动过来。
            Character.motor.SetPositionAndRotation(position, rotation, true);
        }
    }

    private void Update()
    {
        UpdateNetworkTransform();
        if (m_Role != CatNetworkRole.LocalPlayer || Character?.animancer == null)
        {
            return;
        }
        if (m_AnimationState != m_LastAnimationState)
        {
            m_LastAnimationState = m_AnimationState;
            switch (m_AnimationState)
            {
                case CatCharacter.CatAnimationState.Jump:
                    Character.animancer.SwitchAnimation("Jump");
                    break;
                case CatCharacter.CatAnimationState.Idle:
                    Character.animancer.SwitchAnimation("Idle");
                    break;
            }
        }
        switch (m_AnimationState)
        {
            case CatCharacter.CatAnimationState.Crouch:
                Character.animancer.UpdateCrouchVelocity(m_NetworkVelocity, Time.deltaTime);
                break;
            case CatCharacter.CatAnimationState.Walk:
            case CatCharacter.CatAnimationState.Run:
                Character.animancer.UpdateLocomotionVelocity(m_NetworkVelocity, Time.deltaTime);
                break;
        }
    }
    
    private void UpdateNetworkTransform()
    {
        if (!m_HasSnapshot || Character?.motor == null)
        {
            return;
        }
        Vector3 currentPosition =
            Character.motor.TransientPosition;
        Quaternion currentRotation =
            Character.motor.TransientRotation;
        float distanceSqr =
            (m_TargetPosition - currentPosition).sqrMagnitude;
        // 误差太大时直接传送，防止一直追不上服务器。
        if (distanceSqr >=
            m_TeleportDistance * m_TeleportDistance)
        {
            Character.motor.SetPositionAndRotation(
                m_TargetPosition,
                m_TargetRotation,
                true);
            return;
        }
        float positionT = 1f - Mathf.Exp(
            -m_PositionSharpness * Time.deltaTime);
        float rotationT = 1f - Mathf.Exp(
            -m_RotationSharpness * Time.deltaTime);
        Vector3 smoothPosition = Vector3.Lerp(
            currentPosition,
            m_TargetPosition,
            positionT);
        Quaternion smoothRotation = Quaternion.Slerp(
            currentRotation,
            m_TargetRotation,
            rotationT);
        Character.motor.SetPositionAndRotation(
            smoothPosition,
            smoothRotation,
            true);
    }

    private void ApplyRole()
    {
        if (Character?.motor == null)
        {
            return;
        }

        if (NetworkUtils.IsServer())
        {
            m_Role = CatNetworkRole.ServerAuthority;
        }
        else
        {
            m_Role = CatNetworkRole.LocalPlayer;
        }
        // 远端代理不参与本地KCC模拟。
        Character.motor.enabled = m_Role != CatNetworkRole.RemoteProxy;
    }

    private void OnDestroy()
    {
        if (!m_Registered)
        {
            return;
        }

        m_SyncSystem?.UnregisterCat(this);
        m_Registered = false;
    }
}