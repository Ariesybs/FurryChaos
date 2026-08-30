using UnityEngine;

public class CatFsmJump : CatFsmBase
{
    private CatJumpLink m_JumpLink;
    private Vector3 m_StartPosition;
    private Vector3 m_JumpUp;
    private float m_ElapsedTime;
    private float m_JumpChargeTimer;
    private bool m_IsFinished;
    private bool m_StartJump;

    public CatFsmJump(CatCharacter cat) : base(cat)
    {
        CurState = CatFSM.State.Jump;
    }

    public override void OnEnter(CatFSM.State fromState, object enterArg = null)
    {
        base.OnEnter(fromState, enterArg);

        m_JumpLink = enterArg as CatJumpLink;
        m_ElapsedTime = 0f;
        m_IsFinished = m_JumpLink == null;

        if (m_IsFinished)
        {
            return;
        }

        cat.input.LockInput(true);

        m_StartPosition = cat.motor.TransientPosition;
        m_JumpUp = cat.motor.CharacterUp.normalized;
        
        cat.animancer.SwitchAnimation("Jump");
    }

    public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        base.UpdateVelocity(ref currentVelocity, deltaTime);

        if (m_JumpLink == null || m_IsFinished)
        {
            currentVelocity = Vector3.zero;
            return;
        }

        // 等待蓄力完成
        if (m_JumpChargeTimer < cat.moveConfig.JumpChargeTime)
        {
            currentVelocity = Vector3.zero;
            m_JumpChargeTimer += deltaTime;
            return;
        }

        // 开始跳跃
        if (!m_StartJump)
        {
            // 防止 KCC 在起跳的第一帧继续吸附在地面上。
            cat.motor.ForceUnground();
            // 保持移动碰撞开启
            cat.motor.SetMovementCollisionsSolvingActivation(false);
            m_StartJump = true;
        }

        float duration = Mathf.Max(m_JumpLink.Duration, 0.1f);
        m_ElapsedTime = Mathf.Min(m_ElapsedTime + deltaTime, duration);

        float normalizedTime = m_ElapsedTime / duration;
        Vector3 targetPosition =
            m_JumpLink.EvaluatePosition(m_StartPosition, m_JumpUp, normalizedTime);

        // KCC 会在本次模拟中用该速度移动到曲线的下一个采样点。
        currentVelocity =
            (targetPosition - cat.motor.TransientPosition) /
            Mathf.Max(deltaTime, 0.0001f);

        if (normalizedTime >= 1f)
        {
            m_IsFinished = true;
        }
    }

    public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        base.UpdateRotation(ref currentRotation, deltaTime);

        if (m_JumpLink == null)
        {
            return;
        }

        Vector3 lookDirection = Vector3.ProjectOnPlane(
            m_JumpLink.transform.position - cat.motor.TransientPosition,
            m_JumpUp);

        if (lookDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(lookDirection.normalized, m_JumpUp);
        float interpolation =
            1f - Mathf.Exp(-cat.moveConfig.RotationSharpness * deltaTime);

        currentRotation =
            Quaternion.Slerp(currentRotation, targetRotation, interpolation);
    }

    public override void AfterCharacterUpdate(float deltaTime)
    {
        base.AfterCharacterUpdate(deltaTime);

        if (!m_IsFinished)
        {
            return;
        }
        SwitchState(CurState, CatFSM.State.Locomotion);
    }

    public override void OnExit()
    {
        base.OnExit();
        cat.motor.SetMovementCollisionsSolvingActivation(true);
        m_JumpLink = null;
        m_ElapsedTime = 0f;
        m_JumpChargeTimer = 0f;
        m_IsFinished = false;
        m_StartJump = false;
        cat.jumpScanner?.ClearCurrentLink();
        cat.input.LockInput(false);
    }
}