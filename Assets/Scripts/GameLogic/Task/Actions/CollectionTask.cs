using System;
using Sirenix.OdinInspector;

public class CollectionTask : TaskUnit
{
    [LabelText("目标收集物")]
    public ItemData TargetItem; // 目标收集物
    
    [LabelText("目标收集重量/g")]
    public float RequireWeight;

    [NonSerialized]
    private float CurrentWeight;

    public override void Init()
    {
        GameEvent.Register<MsgDeliverPointInteracted>(OnCollectionTrigger);
    }

    private void OnCollectionTrigger(MsgDeliverPointInteracted msg)
    {
        var player = PlayerMgr.Instance.GetPlayer(msg.PlayerNetId);
        var bag = player?.Bag;
        if (bag == null) return;
        var totalWeight = bag.GetItemTotalWeight(TargetItem.Id);
        CurrentWeight += totalWeight;
        GameEvent.Trigger(new MsgGameTaskUpdate());
    }

    public override bool Done()
    {
        return CurrentWeight >= RequireWeight;
    }

    public override float GetProgress()
    {
        return CurrentWeight / RequireWeight;
    }

    public override string GetProgressStr()
    {
        return $"{(CurrentWeight * 100f / RequireWeight):0.##}%";
    }
}