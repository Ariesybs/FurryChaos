public interface ISystem
{
    public void OnInit();
    public void OnAfterAllSystemInit();
    
    public void OnUpdate(float deltaTime);
    
    public void OnFixedUpdate(float deltaTime);
    public void OnPause();
    public void OnResume();
    public void OnDispose();
}