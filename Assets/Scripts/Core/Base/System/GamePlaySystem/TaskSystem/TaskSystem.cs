using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TaskSystem : SubSystem
{
    private TaskConfig m_CurrentTask;
    private GameObject m_TaskSessionGo;
    private TaskSession m_Session;
    public TaskSystem(ISystem mainSystem) : base(mainSystem)
    {
        
    }

    public override void OnInit()
    {
        base.OnInit();
        var cfg = GameRoot.Instance.GameLoader.ConfigLoader.LoadSO<TaskConfig>("task/guide");
        LoadGuideTask().Forget();
    }
    
    private async UniTaskVoid LoadGuideTask()
    {
        var cfg = await GameRoot.Instance.GameLoader.ConfigLoader.LoadSO<TaskConfig>("task/guide");
        if (cfg != null)
        {
            PushGameTask(cfg);
        }
    }

    public void CreateTaskSession()
    {
        m_TaskSessionGo = new GameObject("TaskSession",typeof(TaskSession));
        m_Session = m_TaskSessionGo.GetComponent<TaskSession>();
    }

    public void PushGameTask(TaskConfig config)
    {
        if (config == null)
        {
            return;
        }
        m_CurrentTask = config;
        var taskCnt = config.TaskList.Count;
        for (var i = 0; i < taskCnt; i++)
        {
            var task = config.TaskList[i];
            if (task != null)
            {
                task.Init();
            }
        }

        GameEvent.Trigger(new MsgGameTaskUpdate()
        {
            task = m_CurrentTask
        });
    }

    public TaskConfig GetCurTask()
    {
        return m_CurrentTask;
    }

    public override void OnDispose()
    {
        base.OnDispose();
        if (m_CurrentTask != null)
        {
            
        }
    }
}

public struct MsgGameTaskUpdate : IMsg
{
    public TaskConfig task;
}