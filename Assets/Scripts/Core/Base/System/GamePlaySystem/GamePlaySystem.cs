public class GamePlaySystem : LogicSystem
{
    public ItemSystem ItemSystem;
    public TaskSystem TaskSystem;
    public MatchSystem MatchSystem;
    public DeliverySystem DeliverySystem;
    public CaptureSystem CaptureSystem;

    public override void OnInit()
    {
        ItemSystem = new ItemSystem(this);
        TaskSystem = new TaskSystem(this);
        MatchSystem = new MatchSystem(this);
        DeliverySystem = new DeliverySystem(this);
        CaptureSystem = new CaptureSystem(this);
        SubSystems.Add(ItemSystem);
        SubSystems.Add(TaskSystem);
        SubSystems.Add(MatchSystem);
        SubSystems.Add(DeliverySystem);
        SubSystems.Add(CaptureSystem);
        base.OnInit();
    }
}