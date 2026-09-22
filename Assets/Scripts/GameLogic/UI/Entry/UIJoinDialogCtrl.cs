using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIJoinDialogCtrl : UIPanel
{
    public override UIDisplayMode DisplayMode => UIDisplayMode.Popup;
    public Button m_CloseBtn;
    public TMP_InputField m_Input;
    public Button m_CancelBtn;
    public Button m_JoinBtn;
    public Button m_JoiningBtn;
    void Start()
    {
        UIUtils.ButtonBindListener(m_CloseBtn,OnCloseClick);
        UIUtils.ButtonBindListener(m_CancelBtn,OnCloseClick);
    }

    private void OnCloseClick()
    {
        CloseSelf();
    }

    
}
