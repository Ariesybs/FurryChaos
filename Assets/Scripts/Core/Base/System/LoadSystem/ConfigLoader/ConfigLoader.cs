using Cysharp.Threading.Tasks;
using UnityEngine;

public class ConfigLoader : SubSystem
{
    private LoadSystem m_MainLoader;
    private const string prefix = "cfg/";
    public ConfigLoader(ISystem mainSystem) : base(mainSystem)
    {
        
    }
    
    public async UniTask<T> LoadJson<T>(string key)
    {
        var text = await m_MainLoader.LoadAsync<TextAsset>(prefix + key);
        return JsonUtility.FromJson<T>(text.text);
    }

    public async UniTask<byte[]> LoadBytes(string key)
    {
        var text = await m_MainLoader.LoadAsync<TextAsset>(prefix + key);
        return text.bytes;
    }

    public async UniTask<T> LoadSO<T>(string key) where T : ScriptableObject
    {
        var so = await m_MainLoader.LoadAsync<T>(prefix + key);
        return so;
    }
}