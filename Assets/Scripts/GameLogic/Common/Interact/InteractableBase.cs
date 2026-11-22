using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InteractableBase : NetworkBehaviour , IInteractable
{
    [SerializeField] private InteractType m_InteractType;
    [SerializeField] private string m_Prompt;
    public InteractType InteractType => m_InteractType;   // 属性只读暴露，供外部访问
    public string Prompt => m_Prompt;
    
    // 高光组件
    [SerializeField]
    private Outline m_Outline;

    private void Awake()
    {
        if (m_Outline == null)
        {
            m_Outline = GetComponent<Outline>();
        }
    }

    public virtual string GetPrompt()
    {
        return Prompt;
    }

    public virtual bool CanInteract(ulong netEntityId)
    {
#if LOCAL_DEBUG
        return true;
#endif
        return true;
    }

    private void Reset()
    {
        m_Outline = GetComponent<Outline>();
    }


    public virtual Vector3 GetPos()
    {
        return transform.position;
    }

    public void SetHighLight(bool enable)
    {
        Utils.SetCompActive(m_Outline,enable);
    }

    public virtual void OnPressInteract(ulong netEntityId)
    {
        if (IsSpawned)
        {
            PressInteractServerRpc();
        }
    }

    public virtual void OnHoldInteract(ulong netEntityId)
    {
        
    }
    
    
    [ServerRpc(RequireOwnership = false)]
    protected void PressInteractServerRpc(ServerRpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        var player = PlayerMgr.Instance.GetPlayer(clientId);
        if (player == null)
        {
            return;
        }
        if (!CanInteract(clientId))
        {
            return;
        }
        ServerOnPressInteract(clientId);
    }
    /// <summary>服务端校验通过后的实际交互逻辑，由子类实现。</summary>
    protected virtual void ServerOnPressInteract(ulong clientId)
    {
        
    }
}
