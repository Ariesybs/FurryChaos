public class SubSystem : ISystem
{
    protected virtual bool IsInitialized { get; set; }
    protected virtual string SystemTag { set; get; }
    protected ISystem MainSystem;
    public SubSystem(ISystem mainSystem)
    {
        MainSystem = mainSystem;
    }
    public virtual void OnInit()
    {
        
    }

    public virtual void OnAfterAllSystemInit()
    {
        
    }

    public virtual void OnUpdate(float deltaTime)
    {
        
    }

    public virtual void OnFixedUpdate(float deltaTime)
    {
        
    }

    public virtual void OnPause()
    {
        
    }

    public virtual void OnResume()
    {
        
    }

    public virtual void OnDispose()
    {
        
    }
}