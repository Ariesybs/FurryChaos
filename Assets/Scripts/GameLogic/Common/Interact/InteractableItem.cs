using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class InteractableItem : Item, IInteractable
{
    [ShowInInspector]
    public string Prompt
    {
        get { return m_Prompt; }
    }
    [SerializeField]
    private string m_Prompt;
    
    // 高光组件
    [SerializeField]
    private Outline m_Outline;

    private void Awake()
    {
        if (m_Outline == null)
        {
            m_Outline = GetComponent<Outline>();
        }
        SetHighLight(false);
    }

    public string GetPrompt()
    {
        return Prompt;
    }

    public bool CanInteract(ulong netEntityId)
    {
#if LOCAL_DEBUG
        return true;
#endif
        return true;
    }

    public InteractType InteractType { get; }
    public Vector3 GetPos()
    {
        return transform.position;
    }

    public void SetHighLight(bool enable)
    {
        Utils.SetCompActive(m_Outline,enable);
    }

    public virtual void OnPressInteract(ulong netEntityId)
    {
        
    }

    public virtual void OnHoldInteract(ulong netEntityId)
    {
        
    }
}