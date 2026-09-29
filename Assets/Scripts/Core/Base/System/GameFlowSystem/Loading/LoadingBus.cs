using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public class LoadingBus
{
    private readonly List<ILoadingTask> m_Tasks = new();

    public void Add(ILoadingTask task)
    {
        m_Tasks.Add(task);
    }

    public async UniTask Run(Action<float> onProgress, CancellationToken ct)
    {
        if (m_Tasks.Count == 0)
        {
            // 无任务
            onProgress?.Invoke(1f);
            return;
        }
        var step = 1f / m_Tasks.Count;
        for (var i = 0; i < m_Tasks.Count; i++)
        {
            var task = m_Tasks[i];
            var idx = i;
            await task.Run(new Progress<float>(p =>
            {
                onProgress?.Invoke(idx * step + p * step);
            }),ct);
        }
        // 所有任务已完成
        onProgress?.Invoke(1f);
    }
    
}