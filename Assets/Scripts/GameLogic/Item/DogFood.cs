public class DogFood : InteractableItem
{
    
    public override void OnPressInteract(ulong netEntityId)
    {
        base.OnPressInteract(netEntityId);
        var player = PlayerMgr.Instance.GetPlayer(netEntityId);
        if (player == null)
        {
            return;
        }

        var bagComp = player.Bag;
        if (bagComp == null)
        {
            return;
        }

        bagComp.TryAddItemToBag(this);
    }
}