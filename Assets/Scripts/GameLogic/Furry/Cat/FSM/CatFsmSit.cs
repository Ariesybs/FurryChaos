public class CatFsmSit : CatFsmBase
{
    public CatFsmSit(CatCharacter cat) : base(cat)
    {
        CurState = CatFSM.State.Sit;
    }

    public override void OnEnter(CatFSM.State fromState)
    {
        base.OnEnter(fromState);
        switch (fromState)
        {
            case CatFSM.State.Idle:
                ProcessFromIdleState();
                break;
        }
    }

    public override void OnInput(InputCmd cmd)
    {
        base.OnInput(cmd);
        if (cmd.IsPressed(InputAction.Jump))
        {
            SwitchState(CurState,CatFSM.State.Idle);
        }
    }

    private void ProcessFromIdleState()
    {
        cat.animancer.PlaySequence(new []
        {
            "Idle_To_Sit",
            "Sit_Idle"
        });
    }
}