using UnityEditor;
using UnityEngine;

// 三个表面对应的 Scene 视图编辑手柄，仿 ProBuilder：
// - Box：拖动四条边的中点，该边沿法线移动、对边保持不动（非对称缩放）
// - Circle：拖动圆周手柄改半径
// - Polygon：拖动各顶点（锁定在表面平面内）
// 所有修改都通过 SerializedProperty 写入，自动支持 Ctrl+Z 撤销与 Prefab 覆盖。

/// <summary>
/// Box 表面：四条边的中点拖拽手柄，ProBuilder 式非对称缩放——对边保持不动，
/// 拖动结束后 Transform 轴心自动移动到新的矩形中心（数据里不存 center 偏移）。
/// </summary>
[CustomEditor(typeof(BoxLandingSurface))]
public sealed class BoxLandingSurfaceSceneEditor : Editor
{
    private const float MinEdgeLength = 0.05f; // 防止拖过对边导致矩形翻转
    private const float SnapStep = 0.25f;      // 吸附步长（米）

    private SerializedProperty m_SizeProp;

    private void OnEnable()
    {
        m_SizeProp = serializedObject.FindProperty("m_Size");
    }

    private void OnSceneGUI()
    {
        if (m_SizeProp == null) return;
        var tr = ((BoxLandingSurface)target).transform;

        // 两个轴向 × 正负两侧 = 四条边
        for (int axis = 0; axis < 2; axis++)
        {
            for (int sign = -1; sign <= 1; sign += 2)
            {
                DrawEdgeHandle(tr, axis, sign);
            }
        }
    }

    /// <param name="axis">0 = X 方向边（左右），1 = Z 方向边（前后）</param>
    /// <param name="sign">该边在轴向上的朝向（±1）</param>
    private void DrawEdgeHandle(Transform tr, int axis, int sign)
    {
        var size = m_SizeProp.vector2Value;
        var half = size * 0.5f;

        // 边中点（本地，中心即轴心原点）
        Vector3 localMid = Vector3.zero;
        if (axis == 0) localMid.x = sign * half.x;
        else localMid.z = sign * half.y;

        Vector3 worldPos = tr.TransformPoint(localMid);
        Vector3 worldDir = tr.TransformDirection(axis == 0 ? Vector3.right * sign : Vector3.forward * sign);

        EditorGUI.BeginChangeCheck();
        float handleSize = HandleUtility.GetHandleSize(worldPos) * 0.08f;
        Vector3 newWorld = Handles.Slider(worldPos, worldDir, handleSize, Handles.DotHandleCap, SnapStep);
        if (!EditorGUI.EndChangeCheck()) return;

        // 拖动边的新坐标（轴心本地空间）；对边在 sign 反方向的半边长处，保持不动
        float newCoord = axis == 0
            ? tr.InverseTransformPoint(newWorld).x
            : tr.InverseTransformPoint(newWorld).z;
        float oppositeCoord = -sign * (axis == 0 ? half.x : half.y);

        // 新边长（带最小值钳制），钳制后反推实际边坐标，保证轴心居中计算与实际尺寸一致
        float newLength = Mathf.Max(MinEdgeLength, (newCoord - oppositeCoord) * sign);
        float clampedCoord = oppositeCoord + sign * newLength;

        // 新矩形中心在当前轴心本地空间中的偏移
        float centerOffset = (clampedCoord + oppositeCoord) * 0.5f;
        Vector3 localOffset = Vector3.zero;
        if (axis == 0) localOffset.x = centerOffset;
        else localOffset.z = centerOffset;

        // 轴心自动居中：移动 Transform 到新中心，对边在世界空间中因此保持不动。
        // 与 m_Size 的 SerializedProperty 撤销同处一个撤销组，Ctrl+Z 一次回退。
        Undo.RecordObject(tr, "Resize Landing Surface");
        tr.position += tr.TransformVector(localOffset);

        serializedObject.Update();
        m_SizeProp.vector2Value = axis == 0
            ? new Vector2(newLength, size.y)
            : new Vector2(size.x, newLength);
        serializedObject.ApplyModifiedProperties();
    }
}

/// <summary>Circle 表面：圆周半径手柄。</summary>
[CustomEditor(typeof(CircleLandingSurface))]
public sealed class CircleLandingSurfaceSceneEditor : Editor
{
    private SerializedProperty m_RadiusProp;

    private void OnEnable()
    {
        m_RadiusProp = serializedObject.FindProperty("m_Radius");
    }

    private void OnSceneGUI()
    {
        if (m_RadiusProp == null) return;
        var tr = ((CircleLandingSurface)target).transform;

        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        // RadiusHandle 自带圆周线与拖拽点，旋转用物体朝向保证手柄贴在表面平面上
        float newRadius = Handles.RadiusHandle(tr.rotation, tr.position, m_RadiusProp.floatValue);
        if (EditorGUI.EndChangeCheck())
        {
            m_RadiusProp.floatValue = Mathf.Max(0.01f, newRadius);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

/// <summary>Polygon 表面：逐顶点拖拽，移动锁定在表面平面内。</summary>
[CustomEditor(typeof(PolygonLandingSurface))]
public sealed class PolygonLandingSurfaceSceneEditor : Editor
{
    private const float SnapStep = 0.25f;

    private SerializedProperty m_PointsProp;

    private void OnEnable()
    {
        m_PointsProp = serializedObject.FindProperty("m_Points");
    }

    private void OnSceneGUI()
    {
        if (m_PointsProp == null) return;
        var tr = ((PolygonLandingSurface)target).transform;

        serializedObject.Update();
        for (int i = 0; i < m_PointsProp.arraySize; i++)
        {
            var pointProp = m_PointsProp.GetArrayElementAtIndex(i);
            var local = pointProp.vector2Value;
            Vector3 worldPos = tr.TransformPoint(new Vector3(local.x, 0f, local.y));

            EditorGUI.BeginChangeCheck();
            float handleSize = HandleUtility.GetHandleSize(worldPos) * 0.08f;
            // Slider2D 限制在（right, forward）张成的平面内拖动，up 为平面法线
            Vector3 newWorld = Handles.Slider2D(
                worldPos, tr.up, tr.right, tr.forward,
                handleSize, Handles.DotHandleCap, Vector2.one * SnapStep);
            if (!EditorGUI.EndChangeCheck()) continue;

            // 写回时丢弃 y，顶点始终贴在本地 y=0 平面上
            Vector3 newLocal = tr.InverseTransformPoint(newWorld);
            pointProp.vector2Value = new Vector2(newLocal.x, newLocal.z);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
