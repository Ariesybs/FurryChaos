using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyCtrl : UIPanel
{
    public override UIDisplayMode DisplayMode => UIDisplayMode.FullScreen;
    public Button m_BackBtn;
    public Button m_ReadyBtn;
    public Button m_StartBtn;
    void Start()
    {
        UIUtils.ButtonBindListener(m_BackBtn,OnBackClick);
        UIUtils.ButtonBindListener(m_ReadyBtn,OnReadyClick);
        UIUtils.ButtonBindListener(m_StartBtn,OnStartGameClick);
    }

    private void OnBackClick()
    {
        CloseSelf();
        GameRoot.Instance.GameFlow.LobbySystem.LeaveRoom();
    }

    private void OnReadyClick()
    {
        
    }

    private void OnStartGameClick()
    {
        CloseSelf();
        GameRoot.Instance.GameFlow.LobbySystem.StartGame();
    }

}
