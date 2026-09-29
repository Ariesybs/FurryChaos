using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public class NetworkSceneLoadTask : ILoadingTask
{
    public string Name => "NetworkSceneLoadTask";
    private readonly NetworkSystem m_Network;
    private readonly string m_SceneName;
    
    public NetworkSceneLoadTask(string sceneName)
    {
        m_Network = GameRoot.Instance.GameNet;
        m_SceneName = sceneName;
    }
    public async UniTask Run(IProgress<float> progress, CancellationToken ct)
    {
        var done = new UniTaskCompletionSource();
        
        // 局部回调
        void OnCompleted(string scene, IReadOnlyList<ulong> clients)
        {
            if (scene == m_SceneName)
            {
                done.TrySetResult();
            }
        }
        
        m_Network.SceneLoadProgress += (scene, p) =>
        {
            if (scene == m_SceneName)
            {
                progress?.Report(p);
            }
        };
        
        m_Network.LoadCompleted += OnCompleted;
        
        try
        {
            if (m_Network.IsServer)
            {
                m_Network.LoadScene(m_SceneName);
            }
            await done.Task.AttachExternalCancellation(ct);
            progress?.Report(1f);
            Log.Info(Name,$"场景加载完毕");
        }
        finally
        {
            m_Network.LoadCompleted -= OnCompleted;
        }
    }
}