using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>一份合约的静态配置。不再支持任务链，多份合约并行。</summary>
[CreateAssetMenu(menuName = "GameData/Task Data")]
public class TaskConfig : ScriptableObject
{
    [LabelText("合约ID")]
    public int Id;
    [LabelText("合约标题")]
    public string Title;
    [LabelText("合约描述")]
    [TextArea]
    public string Description;

    [Header("合约目标--可同时进行")] [SerializeReference]
    [LabelText("目标列表")]
    public List<TaskUnit> TaskList = new();

    [Header("合约奖励")] [SerializeReference]
    [LabelText("奖励列表")]
    public List<TaskAward> AwardList = new();

    /// <summary>完成后计入结算总池的金额。</summary>
    public int GetCoinReward()
    {
        var total = 0;
        foreach (var award in AwardList)
        {
            if (award != null)
            {
                total += award.GetCoinReward();
            }
        }

        return total;
    }
}