using KinematicCharacterController;
using UnityEngine;
/// <summary>
/// Idle状态
/// </summary>
public class CatFsmIdle : CatFsmBase
{
    public CatFsmIdle(CatCharacter cat) : base(cat)
    {
        CurState = CatFSM.State.Idle;
    }

    public override void OnEnter(CatFSM.State fromState)
    {
        base.OnEnter(fromState);
        switch (fromState)
        {
            case CatFSM.State.None:
                cat.animancer.SwitchAnimation("Idle");
                break;
            case CatFSM.State.Sit:
                ProcessStateFormSit();
                break;
        }
        
    }

    private void ProcessStateFormSit()
    {
        cat.animancer.PlaySequence(new []
        {
            "Sit_To_Idle",
            "Idle"
        });
    }

    public override void OnInput(InputCmd cmd)
    {
        base.OnInput(cmd);
        if (cmd.IsPressed(InputAction.Sit))
        {
            SwitchState(CurState, CatFSM.State.Sit);
        }
    }
}