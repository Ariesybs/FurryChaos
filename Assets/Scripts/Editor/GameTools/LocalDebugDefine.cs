using UnityEditor;
using UnityEngine;

public static class LocalDebugDefine
{
    private const string Define = "LOCAL_DEBUG";
    private const string MenuPath = "GameTools/本地调试/本地离线调试";

    [MenuItem(MenuPath)]
    public static void Toggle()
    {
        var group = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);

        if (defines.Contains(Define))
        {
            // 当前已启用 → 禁用
            defines = defines.Replace(";" + Define, "").Replace(Define, "");
            Debug.Log("已禁用 本地离线调试");
        }
        else
        {
            // 当前未启用 → 启用
            defines += string.IsNullOrEmpty(defines) ? Define : ";" + Define;
            Debug.Log("已启用 本地离线调试");
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
    }

    [MenuItem(MenuPath, true)]
    public static bool ToggleValidate()
    {
        // 更新菜单勾选状态
        var group = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        Menu.SetChecked(MenuPath, defines.Contains(Define));
        return true;
    }
}