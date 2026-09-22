public interface IGameFlowState
{
    void OnEnter();
    void OnUpdate(float deltaTime);
    void OnExit();
}