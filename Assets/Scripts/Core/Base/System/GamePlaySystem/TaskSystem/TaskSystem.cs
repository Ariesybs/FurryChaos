// Assets/Scripts/Core/Base/System/GamePlaySystem/TaskSystem/TaskSystem.cs
using System.Collections.Generic;

/// <summary>
/// 合约系统。服务端：按带入的合约 Id 创建运行时、推进并在完成时把奖励计入结算总池；
/// 客户端：UI 通过 Session 和 GetConfig 读取复制状态。
/// 合约是可选加成，不影响能否发车。
/// </summary>
public class TaskSystem : SubSystem
{
    private const string Tag = "TaskSystem";
    private readonly List<TaskRuntime> m_Runtimes = new(); // 仅服务端有内容
    private TaskSession m_Session;

    public TaskSession Session => m_Session;

    public TaskSystem(ISystem mainSystem) : base(mainSystem)
    {
    }

    /// <summary>双端按 Id 查合约配置。</summary>
    public static TaskConfig GetConfig(int taskId)
    {
        var taskDB = GameData.GetDB<TaskDB>();
        return taskDB != null ? taskDB.GetById(taskId) : null;
    }

    public void RegisterSession(TaskSession session)
    {
        m_Session = session;
        m_Session.Changed += OnSessionChanged;
        OnSessionChanged(); // 晚加入：立即按当前复制状态刷新
    }

    public void UnregisterSession(TaskSession session)
    {
        if (m_Session != session)
        {
            return;
        }

        // 网络对象正在销毁，只清本地运行时，不再写网络状态
        foreach (var runtime in m_Runtimes)
        {
            runtime.ServerDeactivate(TaskState.Failed);
        }

        m_Runtimes.Clear();
        m_Session.Changed -= OnSessionChanged;
        m_Session = null;
    }

    public void ServerStartContracts(List<int> taskIds)
    {
        if (m_Session == null || !m_Session.IsServer)
        {
            return;
        }

        m_Runtimes.Clear();
        foreach (var taskId in taskIds)
        {
            var config = GetConfig(taskId);
            if (config == null)
            {
                Log.Error(Tag, $"找不到合约配置: {taskId}");
                continue;
            }

            var runtime = new TaskRuntime(config);
            if (!runtime.HasRequiredObjective)
            {
                Log.Error(Tag, $"合约 {taskId} 没有必做目标，将无法完成");
            }

            m_Runtimes.Add(runtime);
        }

        // 先写好网络摘要并分配下标，再激活目标开始监听事件
        m_Session.ServerSetTasks(m_Runtimes);
        foreach (var runtime in m_Runtimes)
        {
            runtime.ServerActivate(OnObjectiveProgressChanged);
        }
    }

    /// <summary>对局结束时调用，未完成的合约标记为失败。</summary>
    public void ServerStop()
    {
        if (m_Session == null || !m_Session.IsServer)
        {
            return;
        }

        foreach (var runtime in m_Runtimes)
        {
            if (runtime.State != TaskState.Active)
            {
                continue;
            }

            runtime.ServerDeactivate(TaskState.Failed);
            m_Session.ServerSetTaskState(runtime.Config.Id, TaskState.Failed);
        }

        m_Runtimes.Clear();
    }

    private void OnObjectiveProgressChanged(TaskUnitRuntime taskUnit)
    {
        var runtime = taskUnit.Owner;
        if (m_Session == null || runtime == null || runtime.State != TaskState.Active)
        {
            return;
        }

        m_Session.ServerSetProgress(taskUnit.NetIndex, taskUnit.Current);
        if (!runtime.AreRequiredObjectivesCompleted())
        {
            return;
        }

        // 先停用再入账：状态迁移只发生一次，奖励因此不会重复计入
        runtime.ServerDeactivate(TaskState.Completed);
        m_Session.ServerSetTaskState(runtime.Config.Id, TaskState.Completed);

        var inGame = GameRoot.Instance.GameFlow.InGameSystem.Session;
        if (inGame != null)
        {
            inGame.ServerAddContractReward(runtime.Config.GetCoinReward());
        }
    }

    private void OnSessionChanged()
    {
        GameEvent.Trigger(new MsgGameTaskUpdate());
    }
}

/// <summary>合约显示数据有变化，UI 收到后自行从 TaskSystem 拉取。</summary>
public struct MsgGameTaskUpdate : IMsg
{
}