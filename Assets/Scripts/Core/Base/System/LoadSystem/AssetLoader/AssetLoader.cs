using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

public class AssetLoader : SubSystem
{
    private readonly LoadSystem m_MainLoader;
    private const string PrefabPrefix = "prefab/";
    private const string ImagePrefix = "img/";
    private const string AudioPrefix = "audio/";
    private const string VideoPrefix = "video/";

    public AssetLoader(ISystem mainSystem) : base(mainSystem)
    {
        m_MainLoader = (LoadSystem)mainSystem;
    }

    public async UniTask<GameObject> LoadPrefab(string key)
    {
        return await m_MainLoader.LoadAsync<GameObject>(PrefabPrefix + key);
    }
    
    public void ReleasePrefab(string key,GameObject go = null)
    {
        if (go != null)
        {
            Object.Destroy(go);
        }
        m_MainLoader.Release(PrefabPrefix + key);
    }

    public async UniTask<Sprite> LoadSprite(string key)
    {
        return await m_MainLoader.LoadAsync<Sprite>(ImagePrefix + key);
    }

    public async UniTask<AudioClip> LoadAudio(string key)
    {
        return await m_MainLoader.LoadAsync<AudioClip>(AudioPrefix + key);
    }

    public async UniTask<VideoClip> LoadVideo(string key)
    {
        return await m_MainLoader.LoadAsync<VideoClip>(VideoPrefix + key);
    }
}