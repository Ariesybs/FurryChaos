using System.Collections.Generic;

public class TaskSystem : SubSystem
{
    private TaskConfig m_CurrentTask;
    public TaskSystem(ISystem mainSystem) : base(mainSystem)
    {
        
    }

    public void PushGameTask(TaskConfig config)
    {
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
    }
}