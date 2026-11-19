using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class UITaskInfoCtrl : MonoBehaviour
{
    public TextMeshProUGUI m_TaskName;
    private List<TaskUnit> m_TaskList;
    void Start()
    {
        GameEvent.Register<MsgGameTaskUpdate>(OnTaskUpdate);
        var task = GameRoot.Instance.GamePlay?.TaskSystem?.GetCurTask();
        UpdateTaskInfo(task);
    }

    private void OnTaskUpdate(MsgGameTaskUpdate msg)
    {
        TaskConfig task;
        if (msg.task == null)
        {
            task = GameRoot.Instance.GamePlay.TaskSystem.GetCurTask();
            
        }
        else
        {
            task = msg.task;
        }
        UpdateTaskInfo(task);
    }

    private void UpdateTaskInfo(TaskConfig cfg)
    {
        if(cfg == null) return;
        var taskList = cfg.TaskList;
        var sb = new StringBuilder();
        sb.Append(cfg.Title + "\n");
        foreach (var taskUnit in taskList)
        {
            sb.Append(taskUnit.Description + taskUnit.GetProgressStr() + "\n");
        }
        UIUtils.TmpSetText(m_TaskName,sb.ToString());
    }
}
