public class GamePlaySystem : LogicSystem
{
    public ItemSystem ItemSystem;

    public override void OnInit()
    {
        ItemSystem = new ItemSystem(this);
        SubSystems.Add(ItemSystem);
        base.OnInit();
    }
}