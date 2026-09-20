public abstract class GameFlowState : IGameFlowState
{
    protected readonly GameFlowSystem Flow;
    protected NetworkSystem Network =>
        GameRoot.Instance.GameNet;

    protected GameFlowState(GameFlowSystem flow)
    {
        Flow = flow;
    }

    public virtual void Enter() { }
    public virtual void Update(float deltaTime) { }
    public virtual void Exit() { }
}