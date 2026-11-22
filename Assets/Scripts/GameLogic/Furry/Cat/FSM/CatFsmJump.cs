using UnityEngine;

/// <summary>
/// 跳跃状态：执行 LandingSurfaceScanner 产出的 CatJumpPlan。
/// 蓄力 → 沿贝塞尔曲线驱动 KCC → 落地吸附；曲线求值两端确定，服务端权威模拟安全。
/// 空中被障碍挡住导致实际位置偏离曲线过多时，判定跳跃被打断，交回 Locomotion 自由落体。
/// </summary>
public class CatFsmJump : CatFsmBase
{
    [Tooltip("空中实际位置偏离曲线超过该距离（米）判定跳跃被打断")]
    private const float BreakDeviation = 0.3f;

    private CatJumpPlan m_Plan;
    private Vector3 m_StartPosition;
    private Vector3 m_JumpUp;
    private float m_ElapsedTime;
    private float m_JumpChargeTimer;
    private float m_Duration;
    private bool m_IsFinished;
    private bool m_StartJump;

    public CatFsmJump(CatCharacter cat) : base(cat)
    {
        CurState = CatFSM.State.Jump;
    }

    public override void OnEnter(CatFSM.State fromState, object enterArg = null)
    {
        base.OnEnter(fromState, enterArg);

        m_Plan = enterArg as CatJumpPlan;
        if (m_Plan == null)
        {
            SwitchState(CurState, CatFSM.State.Locomotion);
            return;
        }
        m_ElapsedTime = 0f;

        cat.input.LockInput(true);

        m_StartPosition = cat.motor.TransientPosition;
        m_JumpUp = cat.motor.CharacterUp.normalized;

        cat.animancer.SwitchAnimation("Jump");
        m_Duration = m_Plan.EvaluateDuration(m_StartPosition, cat.moveConfig);
    }

    public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        base.UpdateVelocity(ref currentVelocity, deltaTime);

        if (m_Plan == null || m_IsFinished)
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
            m_StartJump = true;
        }

        m_ElapsedTime = Mathf.Min(m_ElapsedTime + deltaTime, m_Duration);
        float normalizedTime = m_ElapsedTime / m_Duration;
        Vector3 expectedPosition = m_Plan.EvaluatePosition(m_StartPosition, m_JumpUp, normalizedTime);

        // 空中碰撞打断：实际位置落后曲线过多（被吊灯、柜顶等挡住），
        // 保持当前速度切回 Locomotion 进入自由落体，不再被曲线拽着走
        if ((cat.motor.TransientPosition - expectedPosition).sqrMagnitude > BreakDeviation * BreakDeviation)
        {
            m_IsFinished = true;
            return;
        }

        // KCC 会在本次模拟中用该速度移动到曲线的下一个采样点。
        currentVelocity = (expectedPosition - cat.motor.TransientPosition) / Mathf.Max(deltaTime, 0.0001f);

        if (normalizedTime >= 1f)
        {
            // 落地吸附：对齐到真实站立点，消除曲线终点与表面的误差
            if (m_Plan.Surface != null)
            {
                var snap = m_Plan.Surface.GetClosestStandPoint(cat.motor.TransientPosition);
                cat.motor.SetPosition(snap);
            }
            m_IsFinished = true;
        }
    }

    public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        base.UpdateRotation(ref currentRotation, deltaTime);

        if (m_Plan == null)
        {
            return;
        }

        Vector3 lookDirection = Vector3.ProjectOnPlane(
            m_Plan.TargetPoint - cat.motor.TransientPosition,
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
        m_Plan = null;
        m_ElapsedTime = 0f;
        m_JumpChargeTimer = 0f;
        m_IsFinished = false;
        m_StartJump = false;
        cat.input.LockInput(false);
    }
}
