using System;
using System.Collections.Generic;

public class LogicSystem : ISystem
{
    protected virtual string SystemTag { get; set;}
    protected readonly List<SubSystem> SubSystems = new();
    public virtual void OnInit()
    {
        foreach (var subSystem in SubSystems)
        {
            subSystem.OnInit();
        }
    }

    public virtual void OnAfterAllSystemInit()
    {
        foreach (var subSystem in SubSystems)
        {
            subSystem.OnAfterAllSystemInit();
        }
    }

    public virtual void OnUpdate(float deltaTime)
    {
        foreach (var subSystem in SubSystems)
        {
            subSystem.OnUpdate(deltaTime);
        }
    }

    public virtual void OnFixedUpdate(float deltaTime)
    {
        foreach (var subSystem in SubSystems)
        {
            subSystem.OnFixedUpdate(deltaTime);
        }
    }

    public virtual void OnPause()
    {
        foreach (var subSystem in SubSystems)
        {
            subSystem.OnPause();
        }
    }

    public virtual void OnResume()
    {
        foreach (var subSystem in SubSystems)
        {
            subSystem.OnResume();
        }
    }

    public virtual void OnDispose()
    {
        foreach (var subSystem in SubSystems)
        {
            subSystem.OnDispose();
        }
    }
}