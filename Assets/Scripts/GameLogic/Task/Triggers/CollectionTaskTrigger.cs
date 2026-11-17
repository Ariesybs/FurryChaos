using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionTaskTrigger : InteractableBase
{
    public override string GetPrompt()
    {
        return Prompt;
    }

    public override bool CanInteract(ulong netEntityId)
    {
        return true;
    }

    
    public override void OnPressInteract(ulong netEntityId)
    {
        if(!IsServer)return;
        
        GameEvent.Trigger(new MsgDeliverPointInteracted()
        {
            PlayerNetId = netEntityId
        });
    }

    public override void OnHoldInteract(ulong netEntityId)
    {
        
    }
}

public struct MsgDeliverPointInteracted : IMsg
{
    public ulong PlayerNetId;      // 谁触发的
}
