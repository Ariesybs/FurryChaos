using System.Text;
using TMPro;
using UnityEngine;

public class UITaskInfoCtrl : MonoBehaviour
{
    public TextMeshProUGUI m_TaskName;

    private void OnEnable()
    {
        GameEvent.Register<MsgGameTaskUpdate>(OnTaskUpdate);
        Refresh(); // 晚加入：直接按当前复制状态重建
    }

    private void OnDisable()
    {
        GameEvent.Unregister<MsgGameTaskUpdate>(OnTaskUpdate);
    }

    private void OnTaskUpdate(MsgGameTaskUpdate msg) => Refresh();

    private void Refresh()
    {
        var session = GameRoot.Instance.GamePlay?.TaskSystem?.Session;
        if (session == null)
        {
            m_TaskName.text = "";
            return;
        }

        var sb = new StringBuilder();
        for (var i = 0; i < session.TaskCount; i++)
        {
            var task = session.GetTask(i);
            var config = TaskSystem.GetConfig(task.TaskId);
            if (config == null)
            {
                continue;
            }

            var stateStr = task.State switch
            {
                TaskState.Active => "(进行中)",
                TaskState.Completed => "（已完成）",
                TaskState.Failed => "（已失败）",
                _ => "",
            };
            sb.AppendLine(config.Title + stateStr);

            // 按 TaskId 过滤出这份合约的目标，再用 ConfigIndex 找回配置条目
            for (var j = 0; j < session.TaskUnitCount; j++)
            {
                var taskUnit = session.GetTaskUnit(j);
                if (taskUnit.TaskId != task.TaskId || taskUnit.ConfigIndex >= config.TaskList.Count)
                {
                    continue;
                }

                var unit = config.TaskList[taskUnit.ConfigIndex];
                if (unit == null)
                {
                    continue;
                }

                var mark = taskUnit.Current >= taskUnit.Target ? "[√] " : "[ ] ";
                sb.AppendLine($"  {mark}{unit.Description} {unit.FormatProgress(taskUnit.Current, taskUnit.Target)}");
            }
        }

        m_TaskName.text = sb.ToString();
    }
}