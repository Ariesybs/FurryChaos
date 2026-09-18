using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkSystem : LogicSystem
{
    public bool IsServer => NetworkManager.Singleton.IsServer;
    private readonly NetworkManager m_NetMgr = NetworkManager.Singleton;
    public Action<string,IReadOnlyList<ulong>> LoadCompleted;
    public Action<ulong> ClientConnected;

    public override void OnAfterAllSystemInit()
    {
        base.OnAfterAllSystemInit();
        m_NetMgr.SceneManager.OnLoadComplete += (id, name, mode) =>
        {
            LoadCompleted?.Invoke(name,m_NetMgr.ConnectedClientsIds);
        };

        m_NetMgr.OnClientConnectedCallback += clientId =>
        {
            ClientConnected?.Invoke(clientId);
        };
    }

    public bool StartHost(ushort port = 7777)
    {
        if (m_NetMgr == null)
        {
            return false;
        }

        return m_NetMgr.StartHost();
    }

    public bool StartClient(string address = "127.0.0.1", ushort port = 7777)
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

    public override void OnDispose()
    {
        base.OnDispose();
        if (m_NetMgr != null)
        {
            m_NetMgr.Shutdown();
        }
    }
}