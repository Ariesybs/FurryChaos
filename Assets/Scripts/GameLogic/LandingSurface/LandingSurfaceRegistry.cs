using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 静态可站立表面注册表。表面在 OnEnable/OnDisable 自动注册注销。
/// 数量级在几百以内时线性平方距离扫描已足够快，无需空间树。
/// </summary>
public static class LandingSurfaceRegistry
{
    private static readonly List<LandingSurface> s_Surfaces = new(64);

    public static void Register(LandingSurface surface)
    {
        if (!s_Surfaces.Contains(surface))
        {
            s_Surfaces.Add(surface);
        }
    }

    public static void Unregister(LandingSurface surface) => s_Surfaces.Remove(surface);

    /// <summary>非分配范围查询，返回命中数量，结果写入 results。</summary>
    public static int QuerySphere(Vector3 center, float radius, LandingSurface[] results)
    {
        int count = 0;
        for (int i = 0; i < s_Surfaces.Count && count < results.Length; i++)
        {
            var s = s_Surfaces[i];
            // 查询球与包围球相交判定：距离² ≤ (两半径之和)²
            float sum = radius + s.BoundsRadius;
            if ((s.BoundsCenter - center).sqrMagnitude <= sum * sum)
            {
                results[count++] = s;
            }
        }
        return count;
    }

    // 站立判定的复用缓冲区，避免每帧分配（仅主线程使用）
    private static readonly LandingSurface[] s_UnderBuffer = new LandingSurface[16];

    /// <summary>
    /// 查找脚点当前站立的表面；多层面叠放（上下铺、叠放的箱子）时取平面离脚最近的一个。
    /// 找不到返回 null。仅主线程调用。
    /// </summary>
    /// <param name="footPos">脚底位置（非角色中心点）</param>
    /// <param name="radius">粗筛半径，需覆盖容差内的水平偏移</param>
    /// <param name="heightTolerance">高度容差，与 LandingSurface.IsOnSurface 一致</param>
    public static LandingSurface FindSurfaceUnder(Vector3 footPos, float radius = 1f, float heightTolerance = 0.15f)
    {
        var count = QuerySphere(footPos, radius, s_UnderBuffer);
        LandingSurface best = null;
        var bestHeightDiff = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            var s = s_UnderBuffer[i];
            if (!s.IsOnSurface(footPos, heightTolerance))
            {
                continue;
            }
            // 叠放的多个面同时命中时，平面离脚最近的才是脚下那块
            float diff = Mathf.Abs(s.transform.InverseTransformPoint(footPos).y);
            if (diff < bestHeightDiff)
            {
                bestHeightDiff = diff;
                best = s;
            }
        }
        return best;
    }
}