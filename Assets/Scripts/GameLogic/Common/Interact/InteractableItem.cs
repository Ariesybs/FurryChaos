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
    public virtual void OnPressInteract(ulong netEntityId)
    {
        
    }

    public virtual void OnHoldInteract(ulong netEntityId)
    {
        
    }
}