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

    #endregion
    
}