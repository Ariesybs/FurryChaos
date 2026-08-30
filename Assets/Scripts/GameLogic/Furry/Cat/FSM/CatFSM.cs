using System.Collections.Generic;
using Animancer;
using KinematicCharacterController;
using UnityEngine;

public sealed class CatFSM
{
    public enum State
    {
        None,
        Locomotion,
        Idle,
        Walk,
        Sit,
        Run,
        Jump,
        Sleep,
        Eat,
        Lie,
    }
    private IFurryFSM m_CurrentState;

    private readonly Dictionary<State, IFurryFSM> m_StateFmsDict = new();
    private CatCharacter m_Cat;
    
    public CatFSM(CatCharacter cat)
    {
        m_Cat = cat;
        m_StateFmsDict.Add(State.Locomotion, new CatFsmLocomotion(cat));
        m_StateFmsDict.Add(State.Walk, new CatFsmWalk(cat));
        m_StateFmsDict.Add(State.Sit, new CatFsmSit(cat));

        foreach (var state in m_StateFmsDict.Values)
        {
            state.OnInit();
        }
    }

    public void OnUpdate()
    {
        m_CurrentState?.OnUpdate();
    }

    public void OnInput(InputCmd cmd)
    {
        m_CurrentState?.OnInput(cmd);
    }

    public void SwitchState(State fromState,State nextState)
    {
        if (!m_StateFmsDict.ContainsKey(nextState))
        {
            return;
        }

        var newState = m_StateFmsDict.Get(nextState);
        m_CurrentState?.OnExit();
        m_CurrentState = newState;
        m_CurrentState.OnEnter(fromState);
    }

    public IFurryFSM GetCurrentFsm()
    {
        return m_CurrentState;
    }
}