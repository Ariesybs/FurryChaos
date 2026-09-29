using UnityEditor;
using UnityEditor.SceneManagement;

public static class OpenStartScene
{
    private const string StartScenePath = "Assets/Scenes/GameEntry.unity";
    
    [MenuItem("GameTools/StarScene")]
    private static void Open()
    {
        if (EditorApplication.isPlaying)
        {
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }
        EditorSceneManager.OpenScene(StartScenePath);
    }
}