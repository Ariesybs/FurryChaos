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
}