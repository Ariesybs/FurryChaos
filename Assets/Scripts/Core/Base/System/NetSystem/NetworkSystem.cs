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
    private readonly NetworkManager m_NetMgr = NetworkManager.Singleton;
    private SteamNetworkingSocketsTransport m_Transport => m_NetMgr.NetworkConfig.NetworkTransport as SteamNetworkingSocketsTransport;
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
    }

    public override void OnAfterAllSystemInit()
    {
        base.OnAfterAllSystemInit();

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
        }
    }

    public bool StartHost()
    {
        if (m_NetMgr == null || m_Transport == null)
        {
            return false;
        }
        var hostSteamId = SteamUser.GetSteamID().m_SteamID;
        Log.Info(SystemTag,$"Host Steam ID {hostSteamId}");
        SteamNetworkingUtils.InitRelayNetworkAccess();
        var isOk = m_NetMgr.StartHost();
        if (isOk)
        {
            m_NetMgr.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        }
        return isOk;
    }
    public bool StartClient(ulong hostSteamId = 0)
    {
        if (m_NetMgr == null || m_Transport == null)
        {
            return false;
        }

        if (hostSteamId != 0)
        {
            m_Transport.ConnectToSteamID = hostSteamId;
        }
        SteamNetworkingUtils.InitRelayNetworkAccess();

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
        }
        if (m_NetMgr != null)
        {
            m_NetMgr.Shutdown();
        }
    }
}