using System;
using System.Collections.Generic;
using Netcode.Transports;
using Steamworks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class NetworkSystem : LogicSystem
{
    [Serializable]
    public enum TransportMode
    {
        Unity,
        Steam
    }
    protected override string SystemTag => "NetSystem";
    public bool IsServer => NetworkManager.Singleton.IsServer;
    public bool IsNetworkRunning  => m_NetMgr != null && m_NetMgr.IsListening; // 服务是否正在运行
    [SerializeField]
    private TransportMode m_TransportMode;

    public TransportMode CurTransportMode => m_TransportMode;
    [SerializeField]
    private SteamNetworkingSocketsTransport m_SteamTransport;
    [SerializeField]
    private UnityTransport m_UnityTransport;
    private NetworkManager m_NetMgr => NetworkManager.Singleton;
    public event Action<string, float> SceneLoadProgress;
    public event Action<string> SceneLoadStarted;
    public event Action<string,IReadOnlyList<ulong>> LoadCompleted;
    public event Action<ulong> OnClientConnected;
    public event Action<ulong> OnClientDisconnected;

    public override void OnInit()
    {
        base.OnInit();
        switch (m_TransportMode)
        {
            case TransportMode.Unity:
            {
                m_NetMgr.NetworkConfig.NetworkTransport = m_UnityTransport;
                break;
            }
            case TransportMode.Steam:
            {
                m_NetMgr.NetworkConfig.NetworkTransport = m_SteamTransport;
                break;
            }
        }

        m_NetMgr.OnServerStarted += () =>
        {
            if (IsServer)
            {
                if (m_NetMgr != null && m_NetMgr.IsListening)
                {
                    m_NetMgr.OnClientConnectedCallback += clientId =>
                    {
                        OnClientConnected?.Invoke(clientId);
                    };

                    m_NetMgr.OnClientDisconnectCallback += clientId =>
                    {
                        OnClientDisconnected?.Invoke(clientId);
                    };
            
                    m_NetMgr.SceneManager.OnSceneEvent += OnSceneEvent;
                }
            }
        };
    }

    public bool StartHost()
    {
        if (m_NetMgr == null )
        {
            return false;
        }
        
        var isOk = m_NetMgr.StartHost();
        if (isOk)
        {
            m_NetMgr.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        }
        return isOk;
    }

    public void StopHost()
    {
        if (m_NetMgr == null )
        {
            return;
        }

        m_NetMgr.Shutdown();
    }
    public bool StartClient(ulong hostSteamId = 0)
    {
        if (m_NetMgr == null)
        {
            return false;
        }
        

        return m_NetMgr.StartClient();
    }

    public void LoadScene(string sceneName)
    {
        if (m_NetMgr == null)
        {
            return;
        }
        m_NetMgr.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
    
    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        switch (sceneEvent.SceneEventType)
        {
            case SceneEventType.Load:
                SceneLoadStarted?.Invoke(sceneEvent.SceneName);
                SceneLoadProgress?.Invoke(sceneEvent.SceneName, 0.1f);
                break;
            case SceneEventType.LoadComplete:
                SceneLoadProgress?.Invoke(sceneEvent.SceneName, 0.7f);
                break;
            case SceneEventType.LoadEventCompleted:
                SceneLoadProgress?.Invoke(sceneEvent.SceneName, 1f);
                break;
        }
    }
    
    private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        LoadCompleted?.Invoke(sceneName, clientsCompleted);
    }

    public override void OnDispose()
    {
        base.OnDispose();
        if (m_NetMgr != null && m_NetMgr.SceneManager != null)
        {
            m_NetMgr.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted; 
            m_NetMgr.SceneManager.OnSceneEvent -= OnSceneEvent;
        }
        if (m_NetMgr != null)
        {
            m_NetMgr.Shutdown();
        }
    }
}