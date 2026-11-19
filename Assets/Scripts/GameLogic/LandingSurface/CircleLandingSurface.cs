using UnityEngine;

/// <summary>
/// 圆形可站立面：本地 y=0 平面上的圆盘。
/// 注意：判定基于半径的纯数学计算，Transform 的非均匀缩放不会把圆变成椭圆，
/// 缩放仅影响注册器粗筛用的包围球。
/// </summary>
public class CircleLandingSurface : LandingSurface
{
    [Tooltip("圆盘半径（米）")]
    [SerializeField, Min(0.01f)] private float m_Radius = 0.5f;

    protected override Bounds ComputeLocalBounds()
    {
        return new Bounds(Vector3.zero, new Vector3(m_Radius * 2f, 0.01f, m_Radius * 2f));
    }

    public override Vector3 GetClosestStandPoint(Vector3 wordPos)
    {
        // 世界坐标 -> 局部坐标，XZ 截断到圆盘内，y 归零
        var local = transform.InverseTransformPoint(wordPos);
        var offset = new Vector2(local.x, local.z);
        if (offset.sqrMagnitude > m_Radius * m_Radius)
        {
            offset = offset.normalized * m_Radius;
        }
        local.x = offset.x;
        local.z = offset.y;
        local.y = 0f;
        return transform.TransformPoint(local);
    }

    protected override bool ContainsXZ(float x, float z)
    {
        // 平方距离比较，避免开方
        return x * x + z * z <= m_Radius * m_Radius;
    }

#if UNITY_EDITOR
    public override void Highlight()
    {
    }

    protected override void DrawAreaGizmos(Color fillColor, Color outlineColor)
    {
        const int segments = 48;

        // 填充：以圆心为顶点的三角形扇，本地空间
        FillMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.TRIANGLES);
        GL.Color(fillColor);
        for (int i = 0; i < segments; i++)
        {
            float a0 = i * Mathf.PI * 2f / segments;
            float a1 = (i + 1) * Mathf.PI * 2f / segments;
            GL.Vertex(Vector3.zero);
            GL.Vertex(new Vector3(Mathf.Cos(a0) * m_Radius, 0f, Mathf.Sin(a0) * m_Radius));
            GL.Vertex(new Vector3(Mathf.Cos(a1) * m_Radius, 0f, Mathf.Sin(a1) * m_Radius));
        }
        GL.End();
        GL.PopMatrix();

        // 描边
        var prevZTest = UnityEditor.Handles.zTest;
        UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        UnityEditor.Handles.color = outlineColor;
        UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, m_Radius);
        UnityEditor.Handles.zTest = prevZTest;
    }
#endif
}
