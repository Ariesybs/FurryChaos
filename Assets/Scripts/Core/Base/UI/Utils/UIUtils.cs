using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class UIUtils
{
    #region Button

    public static void ButtonBindListener(Button btn, UnityAction listener)
    {
        if (btn == null)
        {
            return;
        }
        btn.onClick.AddListener(listener);
    }

    public static void ButtonSetInteractable(Button btn, bool enable)
    {
        if (btn == null)
        {
            return;
        }

        btn.interactable = enable;
    }

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
        SetActive(t.gameObject,enable);
    }

    #endregion
    
}