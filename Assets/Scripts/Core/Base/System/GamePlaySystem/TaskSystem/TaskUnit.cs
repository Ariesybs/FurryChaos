using System;
using Sirenix.OdinInspector;

/// <summary>
/// 任务单元，
/// </summary>
[Serializable]
public abstract class TaskUnit
{
    [LabelText("任务描述")]
    public string Description;
    [LabelText("是否是可选任务")]
    public bool IsOptional;

    public abstract void Init(); // 任务初始化，一般用于注册事件
    public abstract bool Done(); // 任务是否完成
    public abstract float GetProgress(); // 获取任务进度
    public abstract string GetProgressStr(); // 获取任务进度文本
}