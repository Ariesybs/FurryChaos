using UnityEngine;

/// <summary>
/// 可站立表面基类。表面是“本地 y=0 平面 + 2D 形状”的纯数据模型：
/// Transform 的位置即面所在位置，旋转即面的朝向（可做斜面）。
/// 不依赖物理碰撞体，注册/查询/站立判定全部走 LandingSurfaceRegistry 与本地空间数学，
/// 服务端 headless 同样可以运行。
/// </summary>
public abstract class LandingSurface : MonoBehaviour
{
    public uint m_SurfaceId;

    // 注册时缓存的世界空间包围球（静态物体只需算一次）
    public Vector3 BoundsCenter { get; private set; }
    public float BoundsRadius { get; private set; }

    protected virtual void OnEnable()
    {
        // 本地 AABB 的 8 个角点变换到世界后重新包围，得到世界空间包围球
        var local = ComputeLocalBounds();
        var world = new Bounds(transform.TransformPoint(local.center), Vector3.zero);
        for (int i = 0; i < 8; i++)
        {
            var corner = local.center + Vector3.Scale(local.extents, new Vector3(
                (i & 1) == 0 ? 1 : -1,
                (i & 2) == 0 ? 1 : -1,
                (i & 4) == 0 ? 1 : -1));
            world.Encapsulate(transform.TransformPoint(corner));
        }
        BoundsCenter = world.center;
        BoundsRadius = world.extents.magnitude;
        LandingSurfaceRegistry.Register(this);
    }

    protected virtual void OnDisable()
    {
        LandingSurfaceRegistry.Unregister(this);
    }

    /// <summary>表面在本地空间的 AABB（面位于本地 y=0，厚度取极小值即可），用于注册器粗筛。</summary>
    protected abstract Bounds ComputeLocalBounds();

    /// <summary>求世界坐标点在表面上投影后的最近可站立点。</summary>
    public abstract Vector3 GetClosestStandPoint(Vector3 wordPos);

    /// <summary>
    /// 判定世界坐标点（应取脚底位置）是否正站在这个面上。
    /// 模板方法：基类做高度容差判定，形状包含由子类 ContainsXZ 实现；
    /// 个别表面若有特殊高度逻辑（如斜面坡度限制）可整体重写。
    /// </summary>
    /// <param name="footPivot">脚底位置（非角色中心点）</param>
    /// <param name="heightTolerance">脚点偏离平面多少米内算“站上”，建议 0.1~0.2</param>
    public virtual bool IsOnSurface(Vector3 footPivot, float heightTolerance = 0.15f)
    {
        var local = transform.InverseTransformPoint(footPivot);
        if (Mathf.Abs(local.y) > heightTolerance)
        {
            return false;
        }
        return ContainsXZ(local.x, local.z);
    }

    /// <summary>本地 XZ 平面上的 2D 形状包含测试（矩形比半边长、圆形比半径平方、多边形射线法）。</summary>
    protected abstract bool ContainsXZ(float x, float z);

#if UNITY_EDITOR
    public abstract void Highlight();

    [Header("Debug 可视化")]
    [SerializeField] private Color m_FillColor = new Color(0.1f, 0.55f, 1f, 0.3f);   // NavMesh 风格半透明蓝
    [SerializeField] private Color m_OutlineColor = new Color(0.1f, 0.55f, 1f, 0.9f);

    // 填充面材质：ZWrite On + ZTest LEqual 的顶点色着色器，
    // Handles 的实心绘制不写深度，遮挡关系是错的，所以填充改用 GL 三角形自绘
    private static Material s_FillMaterial;
    protected static Material FillMaterial
    {
        get
        {
            if (s_FillMaterial == null)
            {
                s_FillMaterial = new Material(Shader.Find("Hidden/FurryChaos/SurfaceAreaFill"))
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
            return s_FillMaterial;
        }
    }

    private void OnDrawGizmos()
    {
        DrawAreaGizmos(m_FillColor, m_OutlineColor);
    }

    /// <summary>
    /// 编辑器专用：绘制可站立区域（本地 y=0 平面上的 2D 形状）。
    /// 填充用 FillMaterial + GL 三角形（写入深度），描边用 Handles。
    /// </summary>
    protected abstract void DrawAreaGizmos(Color fillColor, Color outlineColor);
#endif
}
