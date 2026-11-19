using UnityEngine;

/// <summary>
/// 多边形可站立面：本地 y=0 平面上按顺序定义的 2D 多边形。
/// 点在多边形内时直接投影，在外时取到各边的最近点。
/// Gizmos 填充绘制（DrawAAConvexPolygon）只支持凸多边形，凹多边形请拆成多个凸面。
/// </summary>
public class PolygonLandingSurface : LandingSurface
{
    [Tooltip("本地 XZ 平面上的多边形顶点，按顺时针或逆时针顺序")]
    [SerializeField] private Vector2[] m_Points =
    {
        new(-0.5f, -0.5f), new(0.5f, -0.5f), new(0.5f, 0.5f), new(-0.5f, 0.5f)
    };

    protected override Bounds ComputeLocalBounds()
    {
        // 由顶点直接构建本地 AABB，不依赖碰撞体
        if (m_Points == null || m_Points.Length == 0)
        {
            return new Bounds(Vector3.zero, new Vector3(0.01f, 0.01f, 0.01f));
        }
        var min = m_Points[0];
        var max = m_Points[0];
        foreach (var p in m_Points)
        {
            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
        }
        var center = new Vector3((min.x + max.x) * 0.5f, 0f, (min.y + max.y) * 0.5f);
        var size = new Vector3(Mathf.Max(max.x - min.x, 0.01f), 0.01f, Mathf.Max(max.y - min.y, 0.01f));
        return new Bounds(center, size);
    }

    public override Vector3 GetClosestStandPoint(Vector3 wordPos)
    {
        var local = transform.InverseTransformPoint(wordPos);
        var p = new Vector2(local.x, local.z);
        var closest = ContainsXZ(p.x, p.y) ? p : ClosestPointOnEdges(p);
        return transform.TransformPoint(new Vector3(closest.x, 0f, closest.y));
    }

    protected override bool ContainsXZ(float x, float z)
    {
        // 射线法奇偶判定：从点向 +X 方向发射线，统计与边的交叉次数
        bool inside = false;
        for (int i = 0, j = m_Points.Length - 1; i < m_Points.Length; j = i++)
        {
            var a = m_Points[i];
            var b = m_Points[j];
            if ((a.y > z) != (b.y > z) &&
                x < (b.x - a.x) * (z - a.y) / (b.y - a.y) + a.x)
            {
                inside = !inside;
            }
        }
        return inside;
    }

    /// <summary>求点到多边形各边的最近点（用于点在多边形外时的投影）。</summary>
    private Vector2 ClosestPointOnEdges(Vector2 p)
    {
        var best = Vector2.zero;
        var bestDistSqr = float.MaxValue;
        for (int i = 0, j = m_Points.Length - 1; i < m_Points.Length; j = i++)
        {
            var a = m_Points[j];
            var b = m_Points[i];
            var ab = b - a;
            // 投影参数 t 截断到 [0,1]，得到线段（而非无限延长线）上的最近点
            float t = ab.sqrMagnitude < 1e-8f ? 0f : Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            var q = a + ab * t;
            float distSqr = (q - p).sqrMagnitude;
            if (distSqr < bestDistSqr)
            {
                bestDistSqr = distSqr;
                best = q;
            }
        }
        return best;
    }

#if UNITY_EDITOR
    public override void Highlight()
    {
    }

    protected override void DrawAreaGizmos(Color fillColor, Color outlineColor)
    {
        if (m_Points == null || m_Points.Length < 3) return;

        // 填充：以顶点 0 为基准的三角形扇（仅凸多边形正确，凹多边形请拆分）
        FillMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.TRIANGLES);
        GL.Color(fillColor);
        var origin = new Vector3(m_Points[0].x, 0f, m_Points[0].y);
        for (int i = 1; i < m_Points.Length - 1; i++)
        {
            GL.Vertex(origin);
            GL.Vertex(new Vector3(m_Points[i].x, 0f, m_Points[i].y));
            GL.Vertex(new Vector3(m_Points[i + 1].x, 0f, m_Points[i + 1].y));
        }
        GL.End();
        GL.PopMatrix();

        // 描边
        var prevZTest = UnityEditor.Handles.zTest;
        UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        var world = new Vector3[m_Points.Length];
        for (int i = 0; i < m_Points.Length; i++)
        {
            world[i] = transform.TransformPoint(new Vector3(m_Points[i].x, 0f, m_Points[i].y));
        }
        UnityEditor.Handles.color = outlineColor;
        UnityEditor.Handles.DrawAAPolyLine(world);
        UnityEditor.Handles.DrawAAPolyLine(world[world.Length - 1], world[0]); // 闭合
        UnityEditor.Handles.zTest = prevZTest;
    }
#endif
}
