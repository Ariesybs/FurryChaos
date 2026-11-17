using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkSingleton<T> : NetworkBehaviour where T : NetworkSingleton<T>
{
    public static T Instance
    {
        get
        {
            if (m_Instance == null && !m_IsShuttingDown)
            {
                m_Instance = FindObjectOfType<T>();
                if (m_Instance == null)
                {
                    var go = new GameObject(typeof(T).Name, typeof(T));
                    m_Instance = go.GetComponent<T>();
                    DontDestroyOnLoad(go);
                }
            }
            return m_Instance;
        }
        private set => m_Instance = value;
    }

    private static T m_Instance;
    private static bool m_IsShuttingDown;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;
        DontDestroyOnLoad(gameObject);
    }

    private void OnApplicationQuit()
    {
        m_IsShuttingDown = true;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (m_Instance == this)
        {
            m_Instance = null;
        }
    }
}