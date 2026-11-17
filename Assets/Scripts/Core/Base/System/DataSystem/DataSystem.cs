using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DataSystem : LogicSystem
{
    public ItemDB ItemDB;
    private readonly Dictionary<Type, ScriptableObject> m_DataDict = new();
    public override void OnInit()
    {
        base.OnInit();
        LoadItemDBAsync().Forget();
    }

    public T GetDataDB<T>() where T : ScriptableObject
    {
        m_DataDict.TryGetValue(typeof(T),out var db);
        return db as T;
    }

    private async UniTaskVoid LoadItemDBAsync()
    {
        var cfg = await GameRoot.Instance.GameLoader.ConfigLoader.LoadSO<ItemDB>("ItemDB");
        if (cfg != null)
        {
            m_DataDict.TryAdd(typeof(ItemDB), cfg);
        }
    }
}