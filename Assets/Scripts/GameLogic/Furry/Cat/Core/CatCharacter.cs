using System;
using Animancer;using KinematicCharacterController;
using UnityEngine;

public sealed class CatCharacter : MonoBehaviour , ICharacterController
{
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
    [HideInInspector]
    public Camera catCam;
    public KinematicCharacterMotor motor;
    private void Awake()
    {
        motor.CharacterController = this;
        catFsm = new CatFSM(this);
        catFsm.SwitchState(CatFSM.State.None,CatFSM.State.Locomotion);
        input = new CatInput();
        catCam = Camera.main;
    }

    private void Start()
    {
        input.LockCursor(true);
    }

    private void Update()
    {
        catFsm?.OnUpdate();

        var cmd = input.ReadCmd();
        catFsm?.OnInput(cmd);
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
}
