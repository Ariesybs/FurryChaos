using System;
using Sirenix.OdinInspector;

/// <summary>
/// 任务单元：配置数据 + 无状态判定，不保存任何本局进度。
/// </summary>
[Serializable]
public abstract class TaskUnit
{
    [LabelText("任务描述")]
    public string Description;
    [LabelText("是否是可选任务")]
    public bool IsOptional;
    [LabelText("要求数量")]
    public int RequiredCount = 1;

    /// <summary>目标值，默认用要求数量。</summary>
    public virtual float GetTargetValue() => RequiredCount;

    /// <summary>一批货物装车时给多少进度，不关心则返回 0。</summary>
    public virtual float EvaluateCargoLoaded(MsgCargoLoaded msg) => 0f;

    /// <summary>客户端用复制来的数值生成显示文本。</summary>
    public virtual string FormatProgress(float current, float target) => $"{current:0.#}/{target:0.#}";

    public virtual TaskUnitRuntime CreateRuntime()
    {
        return new TaskUnitRuntime(this);
    }
}