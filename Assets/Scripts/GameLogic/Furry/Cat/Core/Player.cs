using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private List<Component> m_PlayerComponents = new();

    public BagComponent Bag
    {
        get
        {
            return GetPlayerComponent<BagComponent>();
        }
    }

    protected virtual void Awake()
    {
#if LOCAL_DEBUG
        PlayerMgr.Instance.RegisterPlayer(0,this);
#endif
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        PlayerMgr.Instance.RegisterPlayer(this.OwnerClientId,this);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        PlayerMgr.Instance.UnregisterPlayer(this.OwnerClientId);
    }

    public T GetPlayerComponent<T>() where T : Component
    {
        foreach (var component in m_PlayerComponents)
        {
            if (component as T)
            {
                return component as T;
            }
        }

        return null;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
#if LOCAL_DEBUG
        PlayerMgr.Instance?.UnregisterPlayer(0);
#endif
    }
}