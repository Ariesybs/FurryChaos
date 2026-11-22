// Assets/Scripts/Core/Base/System/GamePlaySystem/TaskSystem/TaskRuntime.cs
using System;
using System.Collections.Generic;

public enum TaskState : byte
{
    Inactive = 0,
    Active = 1,
    Completed = 2,
    Failed = 3,
}

/// <summary>一份合约在本局的权威运行时，只存在于服务端。</summary>
[Serializable]
public sealed class TaskRuntime
{
    private readonly List<TaskUnitRuntime> m_TaskUnitRuntimeList = new();
    private Action<TaskUnitRuntime> m_OnProgress;

    public TaskConfig Config { get; }
    public TaskState State { get; private set; } = TaskState.Inactive;
    public IReadOnlyList<TaskUnitRuntime> Objectives => m_TaskUnitRuntimeList;
    public bool HasRequiredObjective { get; }

    public TaskRuntime(TaskConfig config)
    {
        Config = config;
        for (var i = 0; i < config.TaskList.Count; i++)
        {
            var unit = config.TaskList[i];
            if (unit == null)
            {
                continue; // 配置空项跳过，ConfigIndex 保证客户端仍能对齐
            }

            var taskUnit = unit.CreateRuntime();
            taskUnit.Owner = this;
            taskUnit.ConfigIndex = i;
            m_TaskUnitRuntimeList.Add(taskUnit);
            HasRequiredObjective |= !unit.IsOptional;
        }
    }

    public void ServerActivate(Action<TaskUnitRuntime> onProgress)
    {
        if (State != TaskState.Inactive)
        {
            return;
        }

        State = TaskState.Active;
        m_OnProgress = onProgress;
        foreach (var taskUnit in m_TaskUnitRuntimeList)
        {
            taskUnit.ProgressChanged += m_OnProgress;
            taskUnit.ServerActivate();
        }
    }

    public void ServerDeactivate(TaskState finalState)
    {
        if (State != TaskState.Active)
        {
            return;
        }

        State = finalState;
        foreach (var taskUnit in m_TaskUnitRuntimeList)
        {
            taskUnit.ServerDeactivate();
            taskUnit.ProgressChanged -= m_OnProgress;
        }

        m_OnProgress = null;
    }

    /// <summary>没有必做目标的合约视为配置错误，永不自动完成。</summary>
    public bool AreRequiredObjectivesCompleted()
    {
        if (!HasRequiredObjective)
        {
            return false;
        }

        foreach (var taskUnit in m_TaskUnitRuntimeList)
        {
            if (!taskUnit.Config.IsOptional && !taskUnit.IsCompleted)
            {
                return false;
            }
        }

        return true;
    }
}