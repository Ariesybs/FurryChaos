using System;
using UnityEngine;

/// <summary>
/// 上下文跳跃连接。
/// 当前物体的位置代表起跳区域中心，LandingPoint 代表落点。
/// </summary>
public sealed class CatJumpLink : MonoBehaviour
{
    [Header("触发条件")]
    [Tooltip("距离起跳点多远时可以触发")]
    [Min(0.1f)]
    public float ActivationRadius = 1.5f;

    [Tooltip("允许角色与起跳点之间的最大高度差")]
    [Min(0f)]
    public float MaxActivationHeight = 0.5f;

    [Tooltip("输入方向与落点方向的最低点积。-1 表示不限制，0 表示前方半球")]
    [Range(-1f, 1f)]
    public float MinApproachDot = 0.25f;

    [Header("跳跃轨迹")]
    [Tooltip("完成整个跳跃所需时间")]
    [Min(0.1f)]
    public float Duration = 0.8f;

    [Tooltip("跳跃最高点相对基础轨迹的高度")]
    [Min(0f)]
    public float ArcHeight = 1.5f;

    [Tooltip("X 为归一化时间，Y 为归一化高度")]
    public AnimationCurve HeightCurve = new(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 1f),
        new Keyframe(1f, 0f));

    private Transform m_CatTrans;
    

    /// <summary>判断角色是否处于该连接的触发范围。</summary>
    public bool IsInActivationRange(Vector3 characterPosition, Vector3 characterUp)
    {
        characterUp.Normalize();

        Vector3 offset = characterPosition - transform.position;
        float verticalDistance = Mathf.Abs(Vector3.Dot(offset, characterUp));
        Vector3 planarOffset = Vector3.ProjectOnPlane(offset, characterUp);

        return verticalDistance <= MaxActivationHeight &&
               planarOffset.sqrMagnitude <= ActivationRadius * ActivationRadius;
    }

    private void OnTriggerEnter(Collider other)
    {
        var cat = other.GetComponentInParent<CatCharacter>();
        if (cat != null) m_CatTrans = cat.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        var cat = other.GetComponentInParent<CatCharacter>();
        if (cat != null) m_CatTrans = null;
    }

    /// <summary>判断当前输入方向是否大致朝向落点。</summary>
    public bool MatchesApproachDirection(Vector3 characterPosition, Vector3 desiredDirection, Vector3 characterUp)
    {
        if (desiredDirection.sqrMagnitude < 0.001f)
            return true;

        Vector3 directionToLanding =
            Vector3.ProjectOnPlane(transform.position - characterPosition, characterUp);

        if (directionToLanding.sqrMagnitude < 0.001f)
            return true;

        desiredDirection = Vector3.ProjectOnPlane(desiredDirection, characterUp);

        if (desiredDirection.sqrMagnitude < 0.001f)
            return true;

        float dot = Vector3.Dot(desiredDirection.normalized, directionToLanding.normalized);
        return dot >= MinApproachDot;
    }

    /// <summary>
    /// 根据归一化时间计算跳跃轨迹上的世界坐标。
    /// startPosition 应为跳跃开始时的 KCC 根节点位置。
    /// </summary>
    public Vector3 EvaluatePosition(Vector3 startPosition, Vector3 characterUp, float normalizedTime)
    {
        float t = Mathf.Clamp01(normalizedTime);
        Vector3 basePosition = Vector3.Lerp(startPosition, transform.position, t);
        float height = HeightCurve.Evaluate(t) * ArcHeight;
        return basePosition + characterUp.normalized * height;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, ActivationRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.15f);
        if (m_CatTrans == null) return;
        Vector3 previewStart = m_CatTrans.position;
        Vector3 up = m_CatTrans.up.normalized;
        // 扇形中心必须是猫指向落点的方向。
        Vector3 directionToLanding = Vector3.ProjectOnPlane(transform.position - previewStart, up);
        if (directionToLanding.sqrMagnitude < 0.001f)
            return;
        directionToLanding.Normalize();
        // 允许输入方向范围。
        DrawApproachAngle(previewStart, directionToLanding, up);
        // 猫当前朝向，用红线显示。
        Vector3 catForward = Vector3.ProjectOnPlane(m_CatTrans.forward, up).normalized;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(previewStart, previewStart + catForward);
        // 跳跃轨迹。
        const int segmentCount = 24;
        Vector3 previousPosition = EvaluatePosition(previewStart, up, 0f);
        Gizmos.color = Color.cyan;
        for (int i = 1; i <= segmentCount; i++)
        {
            float t = i / (float)segmentCount;
            Vector3 position =
                EvaluatePosition(previewStart, up, t);
            Gizmos.DrawLine(
                previousPosition,
                position);
            previousPosition = position;
        }
    }

    private void OnValidate()
    {
        Duration = Mathf.Max(0.1f, Duration);
        ArcHeight = Mathf.Max(0f, ArcHeight);
        ActivationRadius = Mathf.Max(0.1f, ActivationRadius);
        MaxActivationHeight = Mathf.Max(0f, MaxActivationHeight);

        if (HeightCurve == null || HeightCurve.length == 0)
        {
            HeightCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0f));
        }
    }
    
    private void DrawApproachAngle(Vector3 origin, Vector3 centerDirection, Vector3 up)
    {
        float halfAngle = Mathf.Acos(
            Mathf.Clamp(MinApproachDot, -1f, 1f)
        ) * Mathf.Rad2Deg;

        float lineLength = Mathf.Max(ActivationRadius * 0.4f, 0.5f);

        Gizmos.color = new Color(1f, 0.4f, 0f);

        // 中心方向。
        Gizmos.DrawLine(origin, origin + centerDirection * lineLength);

        const int segmentCount = 16;

        Vector3 previousPoint = origin + Quaternion.AngleAxis(-halfAngle, up) * centerDirection * lineLength;

        // 绘制扇形圆弧。
        for (int i = 1; i <= segmentCount; i++)
        {
            float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segmentCount);

            Vector3 direction = Quaternion.AngleAxis(angle, up) * centerDirection;

            Vector3 point = origin + direction * lineLength;

            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        // 两侧边界。
        Vector3 leftDirection = Quaternion.AngleAxis(-halfAngle, up) * centerDirection;

        Vector3 rightDirection = Quaternion.AngleAxis(halfAngle, up) * centerDirection;

        Gizmos.DrawLine(origin, origin + leftDirection * lineLength);

        Gizmos.DrawLine(origin, origin + rightDirection * lineLength);
    }
}