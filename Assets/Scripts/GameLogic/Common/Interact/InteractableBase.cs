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

    
    public virtual void OnPressInteract(ulong netEntityId)
    {
        
    }

    public virtual void OnHoldInteract(ulong netEntityId)
    {
        
    }
}
