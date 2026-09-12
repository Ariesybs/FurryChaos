using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance { get; private set; }
    private readonly Dictionary<Type, ISystem> m_GameSystems = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        RegisterSystems();
        InitializeSystems();
    }
    
    private void RegisterSystems()
    {
#if UNITY_SERVER
        RegisterSystem(new LogSystem(GameLogLevel.Info, true));
#else
        RegisterSystem(new LogSystem(GameLogLevel.Debug, true));
#endif
        RegisterSystem(new MsgSystem());
        RegisterSystem(new NetworkSystem());
    }
    private void InitializeSystems()
    {
        foreach (var system in m_GameSystems.Values)
        {
            system.OnInit();
        }
        foreach (var system in m_GameSystems.Values)
        {
            system.OnAfterAllSystemInit();
        }
    }

    private void Update()
    {
        foreach (var system in m_GameSystems.Values)
        {
            system.OnUpdate(Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        foreach (var system in m_GameSystems.Values)
        {
            system.OnFixedUpdate(Time.fixedDeltaTime);
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        foreach (var system in m_GameSystems.Values)
        {
            if (pauseStatus)
            {
                system.OnPause();
            }
            else
            {
                system.OnResume();
            }
        }
    }

    private void OnDestroy()
    {
        DisposeAllSystems();
    }

    private void OnApplicationQuit()
    {
        DisposeAllSystems();
    }

    public void RegisterSystem<T>(T system)  where T : class, ISystem
    {
        if (system == null)
        {
            return;
        }
        m_GameSystems.TryAdd(typeof(T), system);
    }

    public T GetSystem<T>()
    {
        m_GameSystems.TryGetValue(typeof(T), out var system);
        return (T)system;
    }

    private void DisposeAllSystems()
    {
        // 逆序释放
        var systems = m_GameSystems.Values.ToArray();
        for (int i = systems.Length -1 ; i >= 0; i--)
        {
            var system = systems[i];
            system.OnDispose();
        }
    }
}