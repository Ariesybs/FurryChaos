
using System;
using UnityEngine;

/// <summary>
/// 单个合约目标的本局权威状态，只存在于服务端。
/// 通用实现：订阅服务端事件，由配置判定给多少进度。
/// </summary>
public class TaskUnitRuntime
{
    public TaskUnit Config { get; }
    public TaskRuntime Owner { get; internal set; }
    /// <summary>在 TaskConfig.TaskList 中的下标，客户端据此找回对应配置。</summary>
    public int ConfigIndex { get; internal set; }
    /// <summary>在 TaskSession 目标列表中的下标。</summary>
    public int NetIndex { get; internal set; }
    public float Target { get; }
    public float Current { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsCompleted => Current >= Target;

    public event Action<TaskUnitRuntime> ProgressChanged;

    public TaskUnitRuntime(TaskUnit config)
    {
        Config = config;
        Target = Mathf.Max(config.GetTargetValue(), 0.0001f);
    }

    internal void ServerActivate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        OnServerActivate();
    }

    internal void ServerDeactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        OnServerDeactivate();
    }

    /// <summary>注册能推进目标的服务端事件。</summary>
    protected virtual void OnServerActivate()
    {
        GameEvent.Register<MsgCargoLoaded>(OnCargoLoaded);
    }

    /// <summary>与 OnServerActivate 对称地取消注册。</summary>
    protected virtual void OnServerDeactivate()
    {
        GameEvent.Unregister<MsgCargoLoaded>(OnCargoLoaded);
    }

    private void OnCargoLoaded(MsgCargoLoaded msg)
    {
        ServerAddProgress(Config.EvaluateCargoLoaded(msg));
    }

    protected void ServerAddProgress(float delta)
    {
        // 停用或已完成后忽略，防止迟到事件继续推进
        if (!IsActive || IsCompleted || delta <= 0f)
        {
            return;
        }

        Current = Mathf.Min(Current + delta, Target);
        ProgressChanged?.Invoke(this);
    }
}