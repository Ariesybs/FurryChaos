using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMainInGameCtrl : UIPanel
{
    public override UIDisplayMode DisplayMode => UIDisplayMode.FullScreen;

    private void OnEnable()
    {
        GameInput.AddListener(OnActionInput);
    }
    
    private void OnActionInput(GameAction action)
    {
        if (action == GameAction.Pause)
        {
            UIMgr.Open<UISettingCtrl>();
        }
    }
    
    private void OnDisable()
    {
        GameInput.RemoveListener(OnActionInput);
    }
}
