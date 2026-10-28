using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "GameData/Task Data")]
public class TaskConfig : ScriptableObject
{
    [LabelText("任务ID")]
    public int Id;
    [LabelText("任务标题")]
    public string Title;
    [LabelText("任务描述")]
    [TextArea]
    public string Description;

    [Header("任务列表--可同时进行")] [SerializeReference]
    [LabelText("任务列表")]
    public List<TaskUnit> TaskList = new();

    [Header("任务链--链式推进")] [SerializeReference]
    [LabelText("下一任务")]
    public TaskConfig NextTask;
    
    [Header("任务奖励")] [SerializeReference]
    [LabelText("奖励列表")]
    public List<TaskAward> AwardList = new();
}