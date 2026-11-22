// Assets/Scripts/Core/Base/System/GamePlaySystem/TaskSystem/TaskSession.cs
using System;
using System.Collections.Generic;
using Unity.Netcode;

public struct TaskNetState : INetworkSerializable, IEquatable<TaskNetState>
{
    public int TaskId;
    public TaskState State;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref TaskId);
        serializer.SerializeValue(ref State);
    }

    public bool Equals(TaskNetState other) => TaskId == other.TaskId && State == other.State;
}

public struct TaskUnitNetState : INetworkSerializable, IEquatable<TaskUnitNetState>
{
    public int TaskId;
    public byte ConfigIndex;
    public float Current;
    public float Target;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref TaskId);
        serializer.SerializeValue(ref ConfigIndex);
        serializer.SerializeValue(ref Current);
        serializer.SerializeValue(ref Target);
    }

    public bool Equals(TaskUnitNetState other)
    {
        return TaskId == other.TaskId && ConfigIndex == other.ConfigIndex &&
               Current.Equals(other.Current) && Target.Equals(other.Target);
    }
}

/// <summary>
/// 合约进度的网络摘要，挂在 InGameSession 同一个网络对象上。
/// 只有服务端写入；客户端和晚加入者从这里重建合约界面。
/// </summary>
public class TaskSession : NetworkBehaviour
{
    private readonly NetworkList<TaskNetState> m_Tasks = new();
    private readonly NetworkList<TaskUnitNetState> m_TaskUnitList = new();

    public event Action Changed;

    public int TaskCount => m_Tasks.Count;
    public int TaskUnitCount => m_TaskUnitList.Count;
    public TaskNetState GetTask(int index) => m_Tasks[index];
    public TaskUnitNetState GetTaskUnit(int index) => m_TaskUnitList[index];

    public override void OnNetworkSpawn()
    {
        m_Tasks.OnListChanged += OnTasksChanged;
        m_TaskUnitList.OnListChanged += OnTaskUnitChanged;
        GameRoot.Instance.GamePlay.TaskSystem.RegisterSession(this);
    }

    public override void OnNetworkDespawn()
    {
        m_Tasks.OnListChanged -= OnTasksChanged;
        m_TaskUnitList.OnListChanged -= OnTaskUnitChanged;
        GameRoot.Instance.GamePlay.TaskSystem.UnregisterSession(this);
    }

    private void OnTasksChanged(NetworkListEvent<TaskNetState> changeEvent) => Changed?.Invoke();
    private void OnTaskUnitChanged(NetworkListEvent<TaskUnitNetState> changeEvent) => Changed?.Invoke();

    public void ServerSetTasks(IReadOnlyList<TaskRuntime> runtimes)
    {
        if (!IsServer)
        {
            return;
        }

        m_Tasks.Clear();
        m_TaskUnitList.Clear();
        for (var i = 0; i < runtimes.Count; i++)
        {
            var runtime = runtimes[i];
            m_Tasks.Add(new TaskNetState { TaskId = runtime.Config.Id, State = TaskState.Active });
            foreach (var taskUnit in runtime.Objectives)
            {
                taskUnit.NetIndex = m_TaskUnitList.Count;
                m_TaskUnitList.Add(new TaskUnitNetState
                {
                    TaskId = runtime.Config.Id,
                    ConfigIndex = (byte)taskUnit.ConfigIndex,
                    Current = taskUnit.Current,
                    Target = taskUnit.Target
                });
            }
        }
    }

    public void ServerSetProgress(int netIndex, float current)
    {
        if (!IsServer || netIndex < 0 || netIndex >= m_TaskUnitList.Count)
        {
            return;
        }

        var state = m_TaskUnitList[netIndex];
        state.Current = current;
        m_TaskUnitList[netIndex] = state;
    }

    public void ServerSetTaskState(int taskId, TaskState taskState)
    {
        if (!IsServer)
        {
            return;
        }

        for (var i = 0; i < m_Tasks.Count; i++)
        {
            var task = m_Tasks[i];
            if (task.TaskId != taskId)
            {
                continue;
            }

            task.State = taskState;
            m_Tasks[i] = task;
            return;
        }
    }
}