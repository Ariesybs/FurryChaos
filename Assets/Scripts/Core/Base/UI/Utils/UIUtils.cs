using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class UIUtils
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

    #endregion
    
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

    

    public static void SetActive(Component t, bool enable)
    {
        if (t == null)
        {
            return;
        }
        SetActive(t.gameObject,enable);
    }

    #endregion

    #region TMP

    public static void TmpSetText(TextMeshProUGUI tmp, string text)
    {
        if (tmp == null)
        {
            return;
        }

        tmp.text = text;
    }

    #endregion

    #region Image

    public static void ImageSetIcon(Image img, Sprite icon)
    {
        if (img == null || icon == null)
        {
            return;
        }

        img.sprite = icon;
    }

    public static void ImageSetAlpha(Image img, float alpha)
    {
        
    }

    #endregion
    
}