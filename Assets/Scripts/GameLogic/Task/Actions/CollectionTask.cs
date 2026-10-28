using Sirenix.OdinInspector;

public class CollectionTask : TaskUnit
{
    [LabelText("目标收集物")]
    public ItemData TargetItem; // 目标收集物
    
    [LabelText("目标收集重量/g")]
    public float RequireWeight;

    public override void Init()
    {
        
    }

    public override bool Done()
    {
        return false;
    }

    public override float GetProgress()
    {
        return 0;
    }

    public override string GetProgressStr()
    {
        return string.Empty;
    }
}