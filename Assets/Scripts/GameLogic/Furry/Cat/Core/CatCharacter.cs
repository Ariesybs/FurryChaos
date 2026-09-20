using System;
using Animancer;
using Cinemachine;
using KinematicCharacterController;
using Unity.Netcode;
using UnityEngine;

public sealed class CatCharacter : NetworkBehaviour , ICharacterController
{
    public enum CatAnimationState : byte
    {
        Idle,
        Walk,
        Run,
        Crouch,
        Jump,
        Sit,
        Lie
    }
    // 状态机
    public CatFSM catFsm;
    // 动画机
    public CatAnimancer animancer;
    // 运动参数
    public CatMovementProfile moveConfig;
    // 输入
    public CatInput input;
    // 跳跃扫描
    public CatJumpScanner jumpScanner;
    // 相机
    [Header("相机")]
    public Transform camFollowPoint;
    public GameObject followCam;
    [HideInInspector]
    public Camera catCam;
    public KinematicCharacterMotor motor;
    public CatAnimationState CurrentAnimationState { get; set; }
    
    [Header("Debug")]
    public bool enableLocalPredict;

    public bool localMove = true;

    private CatNetworkEntity m_Entity;
    private uint m_InputSequence;
    private void Awake()
    {
        motor.CharacterController = this;
        catFsm = new CatFSM(this);
        catFsm.SwitchState(CatFSM.State.None,CatFSM.State.Locomotion);
        input = new CatInput();
        catCam = Camera.main;
        m_Entity = GetComponent<CatNetworkEntity>();
    }

    private void Start()
    {
        if (m_Entity == null || m_Entity.CanReadLocalInput)
        {
            input.LockCursor(true);
        }
    }

    private void Update()
    {
        catFsm?.OnUpdate();

        if (m_Entity != null && !m_Entity.CanReadLocalInput)
        {
            return;
        }

        var cmd = input.ReadCmd();
        if (catCam != null)
        {
            cmd.CameraYaw = catCam.transform.eulerAngles.y;
        }
        cmd.Sequence = ++m_InputSequence;
        if (m_Entity != null)
        {
            cmd.ClientTick = m_Entity.CurrentNetworkTick;
            m_Entity.SubmitLocalInput(cmd);
        }
        else if (localMove)
        {
            ApplyInput(cmd);
        }
    }

    public void ApplyNetworkInput(InputCmd command)
    {
        catFsm?.OnInput(command);
    }

    private void ApplyInput(InputCmd command)
    {
        if (!enableLocalPredict)
        {
            return;
        }

        ApplyNetworkInput(command);
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        catFsm.GetCurrentFsm()?.UpdateRotation(ref currentRotation, deltaTime);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        catFsm.GetCurrentFsm()?.UpdateVelocity(ref currentVelocity, deltaTime);
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        catFsm.GetCurrentFsm()?.BeforeCharacterUpdate(deltaTime);
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        catFsm.GetCurrentFsm()?.PostGroundingUpdate(deltaTime);
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        catFsm.GetCurrentFsm()?.AfterCharacterUpdate(deltaTime);
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
       return catFsm.GetCurrentFsm()?.IsColliderValidForCollisions(coll) ?? false;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        catFsm.GetCurrentFsm()?.OnGroundHit(hitCollider, hitNormal, hitPoint, ref hitStabilityReport);
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
        catFsm.GetCurrentFsm()?.OnMovementHit(hitCollider, hitNormal, hitPoint, ref hitStabilityReport);
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
        Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        catFsm.GetCurrentFsm()?.ProcessHitStabilityReport(hitCollider, hitNormal, hitPoint, atCharacterPosition, atCharacterRotation, ref hitStabilityReport);
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
        catFsm.GetCurrentFsm()?.OnDiscreteCollisionDetected(hitCollider);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        var followCamObj = Instantiate(followCam);
        if (followCamObj != null)
        {
            var c = followCamObj.GetComponent<CinemachineFreeLook>();
            if (c != null)
            {
                c.Follow = camFollowPoint;
                c.LookAt = camFollowPoint;
            }
        }
    }

    public Transform GetCamFollowPoint()
    {
        return camFollowPoint;
    }
}
