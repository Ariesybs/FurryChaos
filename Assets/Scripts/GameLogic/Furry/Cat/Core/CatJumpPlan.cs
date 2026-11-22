using UnityEngine;

/// <summary>
/// 一次跳跃的执行计划：由 LandingSurfaceScanner 产出，CatFsmJump 消费。
/// 纯数据、无场景依赖，服务端与客户端用同一输入各自构造，结果确定性一致，
/// 因此不需要网络同步（输入驱动 = 确定性重放）。
/// </summary>
public class CatJumpPlan
{
    /// <summary>目标表面（可空，表示非表面跳跃）。</summary>
    public LandingSurface Surface;

    /// <summary>目标站立点（通常为 Surface.GetClosestStandPoint 的结果）。</summary>
    public Vector3 TargetPoint;

    /// <summary>弧线在起终点较高者之上额外抬高的量（米）。</summary>
    public float ArcHeight = 0.2f;

    /// <summary>
    /// 三次贝塞尔求值：P1/P2 沿 up 方向抬高，起跳与落地的切线连续且收尾近乎垂直下落，
    /// 比两段直线插值的“三角弧”自然，起跳不会顿挫。
    /// </summary>
    public Vector3 EvaluatePosition(Vector3 startPos, Vector3 up, float t)
    {
        // 设计弧顶 = 起终点较高者 + ArcHeight
        float apexY = Mathf.Max(startPos.y, TargetPoint.y) + ArcHeight;
        // 两个控制点都抬到弧顶高度：高度差只通过端点本身体现，不再重复叠加
        Vector3 p1 = startPos + up * (apexY - startPos.y);
        Vector3 p2 = TargetPoint + up * (apexY - TargetPoint.y);
        float u = 1f - t;
        return u * u * u * startPos
            + 3f * u * u * t * p1
            + 3f * u * t * t * p2
            + t * t * t * TargetPoint;
    }

    /// <summary>按水平距离与配置的跳跃速度估算时长，钳制在配置的最短/最长时长内。</summary>
    public float EvaluateDuration(Vector3 startPos, CatMovementProfile cfg)
    {
        float distance = Vector3.Distance(startPos, TargetPoint);
        return Mathf.Clamp(distance / cfg.JumpSpeed, cfg.JumpMinDuration, cfg.JumpMaxDuration);
    }
}
