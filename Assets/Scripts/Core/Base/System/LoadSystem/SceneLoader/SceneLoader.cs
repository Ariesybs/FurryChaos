using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneLoader : LogicSystem
{
    public enum LoadState
    {
        Idle,
        Loading,
        WaitingForActivation
    }

    public LoadState State { get; private set; }
    public bool IsLoading => m_Operation != null;
    public float Progress { get; private set; }
    public string TargetScene { get; private set; }

    public event Action<string> LoadStarted;
    public event Action<string, float> LoadProgressChanged;
    public event Action<string> SceneReady;
    public event Action<string, Scene> LoadCompleted;
    public event Action<string, string> LoadFailed;

    private AsyncOperation m_Operation;
    private bool m_SetActiveAfterLoad;
    private float m_LastReportedProgress = -1f;

    /// <summary>
    /// 异步加载场景。
    /// </summary>
    /// <param name="sceneName">Build Settings中的场景名或路径。</param>
    /// <param name="mode">Single或Additive。</param>
    /// <param name="activateImmediately">
    /// false时加载至90%后等待ActivateScene。
    /// </param>
    /// <param name="setActiveAfterLoad">
    /// Additive加载完成后是否设为活动场景。
    /// </param>
    public bool LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, bool activateImmediately = true, bool setActiveAfterLoad = false)
    {
        if (IsLoading)
        {
            Log.Warning($"已有场景正在加载：{TargetScene}");
            return false;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            NotifyFailed(sceneName, "场景名称不能为空");
            return false;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            NotifyFailed(sceneName, $"场景不在Build Settings中：{sceneName}");
            return false;
        }

        try
        {
            TargetScene = sceneName;
            Progress = 0f;
            m_LastReportedProgress = -1f;
            m_SetActiveAfterLoad = setActiveAfterLoad;
            State = LoadState.Loading;

            m_Operation = SceneManager.LoadSceneAsync(sceneName, mode);

            if (m_Operation == null)
            {
                ResetOperation();
                NotifyFailed(sceneName, "Unity未能创建场景加载任务");
                return false;
            }

            m_Operation.allowSceneActivation = activateImmediately;
            LoadStarted?.Invoke(sceneName);
            return true;
        }
        catch (Exception exception)
        {
            ResetOperation();
            Log.Error($"场景加载异常：{sceneName}", exception);
            NotifyFailed(sceneName, exception.Message);
            return false;
        }
    }

    /// <summary>
    /// 激活预加载完成的场景。
    /// </summary>
    public bool ActivateScene()
    {
        if (m_Operation == null || State != LoadState.WaitingForActivation)
        {
            return false;
        }

        State = LoadState.Loading;
        m_Operation.allowSceneActivation = true;
        return true;
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);

        if (m_Operation == null)
        {
            return;
        }

        // Unity加载至0.9表示资源加载完成，等待场景激活。
        var normalizedProgress = Mathf.Clamp01(m_Operation.progress / 0.9f);

        SetProgress(normalizedProgress);

        if (!m_Operation.allowSceneActivation && m_Operation.progress >= 0.9f && State != LoadState.WaitingForActivation)
        {
            State = LoadState.WaitingForActivation;
            SceneReady?.Invoke(TargetScene);
        }

        if (m_Operation.isDone)
        {
            CompleteLoad();
        }
    }

    private void CompleteLoad()
    {
        string sceneName = TargetScene;
        Scene scene = FindLoadedScene(sceneName);

        m_Operation = null;
        State = LoadState.Idle;
        SetProgress(1f);

        if (m_SetActiveAfterLoad &&
            scene.IsValid() &&
            scene.isLoaded)
        {
            SceneManager.SetActiveScene(scene);
        }

        LoadCompleted?.Invoke(sceneName, scene);
    }

    private void SetProgress(float value)
    {
        Progress = value;

        if (Mathf.Abs(Progress - m_LastReportedProgress) < 0.001f)
        {
            // 只有变化时才继续
            return;
        }
        Log.Info($"TargetScene {TargetScene} load progress {Progress}");
        m_LastReportedProgress = Progress;
        LoadProgressChanged?.Invoke(TargetScene, Progress);
    }

    private static Scene FindLoadedScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByPath(sceneName);

        if (scene.IsValid())
        {
            return scene;
        }

        string nameWithoutExtension = Path.GetFileNameWithoutExtension(sceneName);

        return SceneManager.GetSceneByName(nameWithoutExtension);
    }

    private void NotifyFailed(string sceneName, string reason)
    {
        State = LoadState.Idle;
        LoadFailed?.Invoke(sceneName, reason);
        Log.Error($"场景加载失败：{reason}");
    }

    private void ResetOperation()
    {
        m_Operation = null;
        State = LoadState.Idle;
        Progress = 0f;
    }

    public override void OnDispose()
    {
        // Unity的AsyncOperation不支持真正取消。
        // 防止销毁系统时场景永远停留在90%。
        if (m_Operation != null)
        {
            m_Operation.allowSceneActivation = true;
        }

        ResetOperation();

        LoadStarted = null;
        LoadProgressChanged = null;
        SceneReady = null;
        LoadCompleted = null;
        LoadFailed = null;

        base.OnDispose();
    }
}