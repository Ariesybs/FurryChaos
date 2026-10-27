using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InteractableBase : NetworkBehaviour , IInteractable
{
    public virtual InteractType InteractType => InteractType.Press;
    public string prompt;
    public virtual string GetPrompt()
    {
        return prompt;
    }

    public virtual bool CanInteract(ulong netEntityId)
    {
        return true;
    }

    
    public virtual void OnPressInteract(ulong netEntityId)
    {
        
    }

    public virtual void OnHoldInteract(ulong netEntityId)
    {
        
    }
}
