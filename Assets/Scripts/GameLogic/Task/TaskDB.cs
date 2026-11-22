using System.Collections.Generic;
using UnityEngine;

/// <summary>全部合约的目录。双端按 Id 查配置，网络上只传 Id。</summary>
[CreateAssetMenu(menuName = "GameData/TaskDB Data")]
public class TaskDB : ScriptableObject
{
    [SerializeField] private List<TaskConfig> m_Tasks = new();
    private Dictionary<int, TaskConfig> m_TasksById;

    public IReadOnlyList<TaskConfig> Tasks => m_Tasks;

    public TaskConfig GetById(int taskId)
    {
        if (m_TasksById == null)
        {
            m_TasksById = new Dictionary<int, TaskConfig>(m_Tasks.Count);
            foreach (var task in m_Tasks)
            {
                if (task != null && !m_TasksById.TryAdd(task.Id, task))
                {
                    Debug.LogError($"[TaskCatalogData] 重复的合约 Id: {task.Id}", this);
                }
            }
        }

        m_TasksById.TryGetValue(taskId, out var config);
        return config;
    }
}