using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DataSystem : LogicSystem
{
    private readonly Dictionary<Type, ScriptableObject> m_DataDict = new();

    public override void OnInit()
    {
        base.OnInit();
        LoadDBAsync<ItemDB>("ItemDB").Forget();
        LoadDBAsync<TaskDB>("TaskDB").Forget();
    }

    public T GetDataDB<T>() where T : ScriptableObject
    {
        m_DataDict.TryGetValue(typeof(T), out var db);
        return db as T;
    }

    private async UniTaskVoid LoadDBAsync<T>(string key) where T : ScriptableObject
    {
        try
        {
            var db = await GameRoot.Instance.GameLoader.ConfigLoader.LoadSO<T>(key);
            if (db != null)
            {
                m_DataDict.TryAdd(typeof(T), db);
            }
        }
        catch (Exception e)
        {
            Log.Error($"数据表加载失败: {key}", e);
        }
    }
}