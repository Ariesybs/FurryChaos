using Sirenix.OdinInspector;

/// <summary>
/// 收集指定物品到目标重量。进度由通用运行时在装车时累加。
/// </summary>
public class CollectionTask : TaskUnit
{
    [LabelText("目标收集物")]
    public ItemData TargetItem;

    [LabelText("目标收集重量/g")]
    public float RequireWeight;

    public override float GetTargetValue() => RequireWeight;

    public override float EvaluateCargoLoaded(MsgCargoLoaded msg)
    {
        return TargetItem != null && msg.ItemId == TargetItem.Id ? msg.Weight : 0f;
    }

    public override string FormatProgress(float current, float target)
    {
        return $"{current * 100f / target:0.#}%";
    }
}