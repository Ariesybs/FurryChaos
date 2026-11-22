using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FolderInfoEntry
{
    public string Owner;
    [TextArea(3, 8)] public string Notes;
    [TextArea(2, 6)] public string Todo;
}

[Serializable]
public class FolderInfoRecord
{
    public string FolderGuid;
    public FolderInfoEntry Info = new();
}

/// <summary>
/// 全部文件夹维护信息的集中存储（单例资产：Assets/Data/FolderInfoDB.asset）。
/// 以文件夹 GUID 为键：在 Unity 内重命名/移动文件夹，信息不会丢。
/// </summary>
public class FolderInfoDB : ScriptableObject
{
    public List<FolderInfoRecord> Records = new();

    public FolderInfoRecord Find(string guid)
    {
        for (int i = 0; i < Records.Count; i++)
        {
            if (Records[i].FolderGuid == guid)
            {
                return Records[i];
            }
        }
        return null;
    }

    public FolderInfoRecord Add(string guid)
    {
        var record = new FolderInfoRecord { FolderGuid = guid };
        Records.Add(record);
        return record;
    }
}