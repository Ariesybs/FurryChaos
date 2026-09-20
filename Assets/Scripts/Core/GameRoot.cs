using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance { get; private set; }

    #region 游戏系统

    public LogSystem GameLog; // 日志系统
    public SdkSystem GameSkd; // SDK系统
    public GameFlowSystem GameFlow; // 流程系统
    public MsgSystem GameMsg; // 事件系统
    public TimeSystem GameTimer; // 计时系统
    public NetworkSystem GameNet; // 网络系统
    public LoadSystem GameLoader; // 载入系统
    #endregion
    
    private readonly Dictionary<Type, ISystem> m_GameSystems = new();

    private void Awake()
    {
        Application.runInBackground = true;
#if UNITY_SERVER
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
#endif
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
        GameLog = RegisterSystem(new LogSystem(GameLogLevel.Info, true));
#else
        GameLog = RegisterSystem(new LogSystem());
#endif
        GameSkd = RegisterSystem(new SdkSystem());
        GameNet = RegisterSystem(new NetworkSystem());
        GameFlow = RegisterSystem(new GameFlowSystem());
        GameMsg = RegisterSystem(new MsgSystem());
        GameTimer = RegisterSystem(new TimeSystem());
        GameLoader = RegisterSystem(new LoadSystem());
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

    private T RegisterSystem<T>(T system)  where T : class, ISystem
    {
        if (system == null)
        {
            return null;
        }
        m_GameSystems.TryAdd(typeof(T), system);
        return system;
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