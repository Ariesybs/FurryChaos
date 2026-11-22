using UnityEngine;

public static class Utils
{
    #region Common

    public static void SetActive(GameObject go, bool enable)
    {
        if (go == null)
        {
            return;
        }

        go.SetActive(enable);
    }
    
    public static void SetActive(Component t, bool enable)
    {
        if (t == null)
        {
            return;
        }
        SetActive(t.gameObject,enable);
    }

    public static void SetCompActive(MonoBehaviour t, bool enable)
    {
        if (t == null)
        {
            return;
        }

        t.enabled = enable;
    }

    #endregion
}