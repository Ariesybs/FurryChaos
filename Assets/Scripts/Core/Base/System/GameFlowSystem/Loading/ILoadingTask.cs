using System;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// Loading任务接口，所有需要在loading中加载的任务实现该接口并注册进loading总线中。
/// </summary>
public interface ILoadingTask
{
    string Name { get; } // 任务名
    UniTask Run(IProgress<float> progress, CancellationToken ct);
}