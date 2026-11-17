using UnityEngine;

public static class GameData
{
    public static ItemDB ItemDB
    {
        get
        {
            return GetDB<ItemDB>();
        }
    }
    public static T GetDB<T>() where T :ScriptableObject
    {
        var db = GameRoot.Instance.GameData.GetDataDB<T>();
        return db;
    }
}