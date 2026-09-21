using System;
using System.Collections.Generic;

public sealed class UIRegister
{
    private readonly Dictionary<Type, UIPanel> m_Prefabs = new();

    public void Register<T>(T prefab) where T : UIPanel
    {
        Register((UIPanel)prefab);
    }

    public void Register(UIPanel prefab)
    {
        if (prefab == null)
        {
            Log.Warning("UIRegister 忽略空预制体");
            return;
        }

        var type = prefab.GetType();
        if (m_Prefabs.ContainsKey(type))
        {
            Log.Warning($"UIRegister 重复注册 {type.Name}，已覆盖");
        }

        m_Prefabs[type] = prefab;
    }

    public UIPanel GetPrefab<T>() where T : UIPanel
    {
        return GetPrefab(typeof(T));
    }

    public UIPanel GetPrefab(Type type)
    {
        if (type == null)
        {
            return null;
        }

        if (m_Prefabs.TryGetValue(type, out var prefab))
        {
            return prefab;
        }

        foreach (var pair in m_Prefabs)
        {
            if (type.IsAssignableFrom(pair.Key))
            {
                return pair.Value;
            }
        }

        return null;
    }

    public bool IsRegistered<T>() where T : UIPanel
    {
        return GetPrefab<T>() != null;
    }

    public void Clear()
    {
        m_Prefabs.Clear();
    }
}
