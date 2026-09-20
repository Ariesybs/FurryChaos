public interface IGameFlowState
{
    void Enter();
    void Update(float deltaTime);
    void Exit();
}