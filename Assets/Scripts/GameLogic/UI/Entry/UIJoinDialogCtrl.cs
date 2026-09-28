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
        UIUtils.ButtonBindListener(m_JoinBtn,OnJoinClick);
    }

    private void OnJoinClick()
    {
        CloseSelf();
        GameRoot.Instance.GameFlow.LobbySystem.JoinRoom(0);
    }

    private void OnCloseClick()
    {
        CloseSelf();
    }

    
}
