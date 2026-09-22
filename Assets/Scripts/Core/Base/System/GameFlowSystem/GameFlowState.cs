public abstract class GameFlowState : IGameFlowState
{
    protected readonly GameFlowSystem Flow;
    protected NetworkSystem Network =>
        GameRoot.Instance.GameNet;

    protected GameFlowState(GameFlowSystem flow)
    {
        Flow = flow;
    }

    public virtual void OnEnter() { }
    public virtual void OnUpdate(float deltaTime) { }
    public virtual void OnExit() { }
}