using UnityEngine;

/// <summary>
/// 矩形可站立面：本地 y=0 平面上的 XZ 矩形，中心即轴心。
/// 类名保留 BoxLandingSurface 以免破坏场景中已有实例的脚本引用，
/// 但数据模型已是 2D 平面，不再需要 BoxCollider。
/// </summary>
public class BoxLandingSurface : LandingSurface
{
    [Tooltip("矩形尺寸：x 为本地 X 方向边长，y 为本地 Z 方向边长（米）")]
    [SerializeField] private Vector2 m_Size = Vector2.one;

    protected override Bounds ComputeLocalBounds()
    {
        // 厚度取 1cm，仅为让 AABB 合法，站立面本身是零厚度平面
        return new Bounds(Vector3.zero, new Vector3(m_Size.x, 0.01f, m_Size.y));
    }

    public override Vector3 GetClosestStandPoint(Vector3 wordPos)
    {
        // 世界坐标 -> 局部坐标，XZ 截断到矩形内，y 归零（面即本地 y=0 平面）
        var local = transform.InverseTransformPoint(wordPos);
        local.x = Mathf.Clamp(local.x, -m_Size.x * 0.5f, m_Size.x * 0.5f);
        local.z = Mathf.Clamp(local.z, -m_Size.y * 0.5f, m_Size.y * 0.5f);
        local.y = 0f;
        return transform.TransformPoint(local);
    }

    protected override bool ContainsXZ(float x, float z)
    {
        return Mathf.Abs(x) <= m_Size.x * 0.5f && Mathf.Abs(z) <= m_Size.y * 0.5f;
    }

#if UNITY_EDITOR
    public override void Highlight()
    {
    }

    protected override void DrawAreaGizmos(Color fillColor, Color outlineColor)
    {
        var halfX = m_Size.x * 0.5f;
        var halfZ = m_Size.y * 0.5f;

        // 本地 y=0 平面四角，顺序围成一圈
        var corners = new Vector3[]
        {
            new(-halfX, 0f, -halfZ),
            new(-halfX, 0f, halfZ),
            new(halfX, 0f, halfZ),
            new(halfX, 0f, -halfZ),
        };

        // 填充：GL 三角形，材质写入深度，遮挡关系正确
        FillMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.TRIANGLES);
        GL.Color(fillColor);
        EmitTriangle(corners[0], corners[1], corners[2]);
        EmitTriangle(corners[0], corners[2], corners[3]);
        GL.End();
        GL.PopMatrix();

        // 描边：Handles 线框，同样按深度遮挡
        var prevZTest = UnityEditor.Handles.zTest;
        UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        var world = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            world[i] = transform.TransformPoint(corners[i]);
        }
        UnityEditor.Handles.color = outlineColor;
        UnityEditor.Handles.DrawAAPolyLine(world);
        UnityEditor.Handles.DrawAAPolyLine(world[3], world[0]); // 闭合
        UnityEditor.Handles.zTest = prevZTest;
    }

    /// <summary>GL 三角形发射，本地空间顶点，供各子类复用风格保持一致。</summary>
    protected static void EmitTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        GL.Vertex(a);
        GL.Vertex(b);
        GL.Vertex(c);
    }
#endif
}
