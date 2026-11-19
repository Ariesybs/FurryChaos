using System;
using UnityEngine;

/// <summary>
/// 可站立表面扫描器（调试）：走 LandingSurfaceRegistry 纯数学查询，
/// 不依赖物理碰撞体，因此无需 LayerMask，也不会被无关碰撞体挤占缓冲。
/// </summary>
public class LandingSurfaceScanner : MonoBehaviour
{
    [Range(0.1f,3f)]
    public float m_Radius;
    private LandingSurface[] m_Results;

    private void Awake()
    {
        m_Results = new LandingSurface[16];
    }

    private void Update()
    {
        Scan();
    }

    private void Scan()
    {
        var count = LandingSurfaceRegistry.QuerySphere(transform.position, m_Radius, m_Results);
        for (int i = 0; i < count; i++)
        {
            var point = m_Results[i].GetClosestStandPoint(transform.position);
            // ViewDebug.DrawPoint(point, 0.1f, Color.green);
            TipsMgr.ShowTip(new TipsInfo
            {
                TipId = 1,
                TipsType = ENUM_TIPS_TYPE.TIPS_LANDING_SURFACE,
                Anchor = new TipAnchor { FixedPoint = point },
                Text = "跳上桌面",
                KeyHint = "空格",
                Observer = transform,
                ShowDistance = m_Radius,        // 超出 5 米自动隐藏
                HideWhenOccluded = false,  // 被墙挡住自动隐藏
            });
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position,m_Radius);
    }
}
