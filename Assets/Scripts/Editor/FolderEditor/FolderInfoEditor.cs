using UnityEditor;
using UnityEngine;

/// <summary>
/// 文件夹 Inspector：选中任意目录即显示维护信息字段（负责人/说明/TODO）。
/// 留空不产生任何数据；首次输入内容的那一刻才懒创建信息库资产与对应记录。
/// </summary>
[CustomEditor(typeof(DefaultAsset))]
public class FolderInfoEditor : Editor
{
    private const string DbPath = "Assets/Scripts/Data/FolderData/FolderInfoDB.asset";

    // 关掉 Editor 基类的默认预览区（Inspector 底部的黑色矩形）
    public override bool HasPreviewGUI() => false;

    public override void OnInspectorGUI()
    {
        var path = AssetDatabase.GetAssetPath(target);
        if (!AssetDatabase.IsValidFolder(path))
        {
            base.OnInspectorGUI();
            return;
        }

        // Inspector 会把文件夹这类导入资产包在禁用组里，控件只能看不能点；
        // 这里的数据存在 FolderInfoDB 而不是文件夹本身，因此强制启用，画完恢复
        var prevEnabled = GUI.enabled;
        GUI.enabled = true;

        DrawFolderInfo(path);

        GUI.enabled = prevEnabled;
    }

    private void DrawFolderInfo(string path)
    {
        var db = AssetDatabase.LoadAssetAtPath<FolderInfoDB>(DbPath);
        var guid = AssetDatabase.AssetPathToGUID(path);
        var info = db?.Find(guid)?.Info;

        EditorGUILayout.LabelField("文件夹维护信息", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(path, EditorStyles.miniLabel);
        EditorGUILayout.Space(6);

        // 字段始终显示：无记录时用空值，用户留空则不落任何数据
        EditorGUI.BeginChangeCheck();
        var owner = EditorGUILayout.TextField("负责人", info?.Owner ?? "");
        EditorGUILayout.LabelField("说明", EditorStyles.miniBoldLabel);
        var notes = EditorGUILayout.TextArea(info?.Notes ?? "", GUILayout.MinHeight(60));
        EditorGUILayout.LabelField("TODO", EditorStyles.miniBoldLabel);
        var todo = EditorGUILayout.TextArea(info?.Todo ?? "", GUILayout.MinHeight(40));
        if (!EditorGUI.EndChangeCheck())
        {
            return;
        }

        // 首次编辑才落库：DB 资产不存在则此刻创建（全项目只此一个）
        if (db == null)
        {
            db = CreateDB();
        }
        Undo.RecordObject(db, "Edit Folder Info");
        var record = db.Find(guid) ?? db.Add(guid);
        record.Info.Owner = owner;
        record.Info.Notes = notes;
        record.Info.Todo = todo;
        EditorUtility.SetDirty(db);
    }

    private static FolderInfoDB CreateDB()
    {
        EnsureFolder(System.IO.Path.GetDirectoryName(DbPath)?.Replace('\\', '/'));
        var db = CreateInstance<FolderInfoDB>();
        AssetDatabase.CreateAsset(db, DbPath);
        AssetDatabase.SaveAssets();
        return db;
    }
    
    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
        {
            return;
        }
        var parent = System.IO.Path.GetDirectoryName(folder)?.Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
    }
}