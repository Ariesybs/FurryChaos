using UnityEngine;

public class CatFsmLocomotion : CatFsmBase
{
    private InputCmd m_CachedCmd;
    private CatGait m_CatGait;
    private Vector3 m_DesiredMoveDirection;
    public CatFsmLocomotion(CatCharacter cat) : base(cat)
    {
        CurState = CatFSM.State.Locomotion;
    }

    public override void OnEnter(CatFSM.State fromState, object enterArg = null)
    {
        base.OnEnter(fromState, enterArg);
        m_CatGait = CatGait.Idle;
        cat.animancer.UpdateLocomotion(m_CatGait);
    }

    public override void OnInput(InputCmd cmd)
    {
        base.OnInput(cmd);
        m_CachedCmd = cmd;
        if (cmd.IsPressed(InputAction.Jump))
        {
            if (cat.jumpScanner.TryFindBestJumpLink(m_DesiredMoveDirection, out CatJumpLink jumpLink))
            {
                SwitchState(CurState, CatFSM.State.Jump,jumpLink);
                return;
            }
        }
        if (m_CachedCmd.Direction == Vector2.zero)
        {
            m_CatGait = CatGait.Idle;
        }
        else if (cmd.IsHeld(InputAction.Crouch))
        {
            m_CatGait = CatGait.Crouch;
        }
        else if (cmd.IsHeld(InputAction.Run))
        {
            m_CatGait = CatGait.Run;
        }
        else
        {
            m_CatGait = CatGait.Walk;
        }
        
    }

    public override void BeforeCharacterUpdate(float deltaTime)
    {
        base.BeforeCharacterUpdate(deltaTime);
        var cam = cat.catCam;
        if (cam == null || m_CachedCmd.Direction.sqrMagnitude < 0.001f)
        {
            m_DesiredMoveDirection = Vector3.zero;
        }
        var up = cat.motor.CharacterUp;
        var cameraForward = Vector3.ProjectOnPlane(cam.transform.forward, up);
        if (cameraForward.sqrMagnitude < 0.001f)
        {
            cameraForward = Vector3.ProjectOnPlane(cam.transform.up, up);
        }
        cameraForward.Normalize();
        var cameraRight = Vector3.Cross(up, cameraForward).normalized;
        m_DesiredMoveDirection = Vector3.ClampMagnitude(cameraRight * m_CachedCmd.Direction.x + cameraForward * m_CachedCmd.Direction.y, 1f);
    }

    public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        base.UpdateVelocity(ref currentVelocity, deltaTime);
        var moveDirection = m_DesiredMoveDirection;
        if (cat.motor.GroundingStatus.IsStableOnGround && moveDirection.sqrMagnitude > 0f)
        {
            var magnitude = moveDirection.magnitude;
            moveDirection = Vector3.ProjectOnPlane(moveDirection, cat.motor.GroundingStatus.GroundNormal).normalized * magnitude;
        }
        if (cat.motor.GroundingStatus.IsStableOnGround)
        {
            currentVelocity = moveDirection * GetMoveSpeed();
        }
        else
        {
            Vector3 planarVelocity = moveDirection * GetMoveSpeed();
            Vector3 verticalVelocity = Vector3.Project(currentVelocity, cat.motor.CharacterUp);
            currentVelocity = planarVelocity + verticalVelocity + cat.moveConfig.Gravity * deltaTime;
        }
    }
    
    public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        base.UpdateRotation(ref currentRotation, deltaTime);
        Vector3 lookDirection = m_DesiredMoveDirection;
        if (lookDirection.sqrMagnitude < 0.001f) return;
        
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection, cat.motor.CharacterUp);
        float t = 1f - Mathf.Exp(-cat.moveConfig.RotationSharpness * deltaTime);
        currentRotation = Quaternion.Slerp(currentRotation, targetRotation, t);
    }

    public override void AfterCharacterUpdate(float deltaTime)
    {
        base.AfterCharacterUpdate(deltaTime);
        var planarVelocity = Vector3.ProjectOnPlane(cat.motor.Velocity, cat.motor.CharacterUp);
        // 动画更新
        var moveSpeed = planarVelocity.magnitude;
        cat.animancer.UpdateLocomotion(m_CatGait,moveSpeed);
    }

    private float GetMoveSpeed()
    {
        switch (m_CatGait)
        {
            case CatGait.Idle:
                return 0;
            case CatGait.Crouch:
                return cat.moveConfig.CrouchSpeed;
            case CatGait.Walk:
                return cat.moveConfig.WalkSpeed;
            case CatGait.Run:
                return cat.moveConfig.RunSpeed;
        }
        return 0;
    }
}

public enum CatGait
{
    None,
    Idle, // 站立
    Walk, // 移动
    Run, // 奔跑
    Crouch, // 静步
}