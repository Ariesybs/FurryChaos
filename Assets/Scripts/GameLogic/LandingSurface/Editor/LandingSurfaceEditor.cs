using UnityEditor;
using UnityEngine;

/// <summary>
/// Hierarchy 右键 / GameObject 菜单：创建各类可站立表面。
/// 路径 GameObject/LandingSurface/... 会同时出现在顶部菜单和 Hierarchy 右键菜单。
/// 表面是纯数据组件，创建时不再附带任何碰撞体。
/// </summary>
public static class LandingSurfaceEditor
{
    [MenuItem("GameObject/LandingSurface/Box Landing Surface", false, 10)]
    private static void CreateBox(MenuCommand command)
    {
        CreateSurface<BoxLandingSurface>("BoxLandingSurface", command);
    }

    [MenuItem("GameObject/LandingSurface/Circle Landing Surface", false, 10)]
    private static void CreateCircle(MenuCommand command)
    {
        CreateSurface<CircleLandingSurface>("CircleLandingSurface", command);
    }

    [MenuItem("GameObject/LandingSurface/Polygon Landing Surface", false, 10)]
    private static void CreatePolygon(MenuCommand command)
    {
        CreateSurface<PolygonLandingSurface>("PolygonLandingSurface", command);
    }

    /// <summary>
    /// 与内置“创建 Cube”一致的行为：
    /// 右键某物体时创建为其子级（保持世界坐标），支持 Ctrl+Z 撤销，创建后自动选中。
    /// </summary>
    private static T CreateSurface<T>(string name, MenuCommand command) where T : LandingSurface
    {
        var go = new GameObject(name, typeof(T));

        // 尽量出生在 Scene 视图注视点，避免跑到世界原点找不到
        if (SceneView.lastActiveSceneView != null)
        {
            go.transform.position = SceneView.lastActiveSceneView.pivot;
        }

        GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
        UnityEditor.Selection.activeObject = go;   // 创建后自动选中新物体
        return go.GetComponent<T>();
    }
}
