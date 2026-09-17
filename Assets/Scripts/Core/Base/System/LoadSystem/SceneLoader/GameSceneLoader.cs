using System;
using UnityEngine.SceneManagement;

public static class GameSceneLoader
{
    public static bool LoadScene(
        string sceneName,
        LoadSceneMode mode = LoadSceneMode.Single,
        bool activateImmediately = true, 
        bool setActiveAfterLoad = false,
        Action onCompleteLoad = null)
    {
        var gameLoader = GameRoot.Instance.GameLoader;
        if (gameLoader == null)
        {
            return false;
        }

        var sceneLoader = gameLoader.GameSceneLoader;
        if (sceneLoader == null)
        {
            return false;
        }

        return sceneLoader.LoadScene(sceneName, mode, activateImmediately, setActiveAfterLoad);
    }
}