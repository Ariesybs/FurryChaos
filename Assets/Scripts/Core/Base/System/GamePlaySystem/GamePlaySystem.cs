public class GamePlaySystem : LogicSystem
{
    public ItemSystem ItemSystem;
    public TaskSystem TaskSystem;

    public override void OnInit()
    {
        ItemSystem = new ItemSystem(this);
        TaskSystem = new TaskSystem(this);
        SubSystems.Add(ItemSystem);
        SubSystems.Add(TaskSystem);
        base.OnInit();
    }
}