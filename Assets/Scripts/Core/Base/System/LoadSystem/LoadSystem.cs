using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LoadSystem : LogicSystem
{
    public SceneLoader GameSceneLoader;
    public AssetLoader AssetLoader;
    public ConfigLoader ConfigLoader;
    
    private sealed class AssetEntry
    {
        public AsyncOperationHandle Handle;
        public int RefCount;
    }
    
    private readonly Dictionary<string, AssetEntry> m_Assets = new();
    
    // 异步（主路径）
    public async UniTask<T> LoadAsync<T>(string key, CancellationToken ct = default) where T : UnityEngine.Object
    {
        if (!m_Assets.TryGetValue(key, out var entry))
        {
            entry = new AssetEntry
            {
                Handle = Addressables.LoadAssetAsync<T>(key)
            };
            m_Assets.Add(key, entry);
        }
        entry.RefCount++;
        try
        {
            await entry.Handle.ToUniTask(cancellationToken: ct);
            return (T)entry.Handle.Result;
        }
        catch (Exception)
        {
            Release(key); // 这次加载失败，把刚加上的计数还回去
            throw;
        }
    }
    
    public void Release(string key)
    {
        if (string.IsNullOrEmpty(key) || !m_Assets.TryGetValue(key, out var entry))
        {
            return;
        }
        entry.RefCount--;
        if (entry.RefCount > 0)
        {
            return;
        }
        if (entry.Handle.IsValid())
        {
            Addressables.Release(entry.Handle);
        }
        m_Assets.Remove(key);
    }


    public override void OnInit()
    {
        base.OnInit();
        Addressables.InitializeAsync();
        GameSceneLoader = new SceneLoader();
        AssetLoader = new AssetLoader(this);
        ConfigLoader = new ConfigLoader(this);
        GameSceneLoader.OnInit();
        AssetLoader.OnInit();
        ConfigLoader.OnInit();
    }

    public override void OnAfterAllSystemInit()
    {
        base.OnAfterAllSystemInit();
        GameSceneLoader.OnAfterAllSystemInit();
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        GameSceneLoader.OnUpdate(deltaTime);
    }

    public override void OnDispose()
    {
        base.OnDispose();
        GameSceneLoader.OnDispose();
    }
}