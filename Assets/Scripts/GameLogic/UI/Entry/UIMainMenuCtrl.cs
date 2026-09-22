using System;
using UnityEngine;
using UnityEngine.UI;

public struct UIMainMenuSelectOptMsg : IMsg
{
    public enum OptType
    {
        Idle,
        Customization,
        Setting,
        Quit
    }

    public OptType type;
}
public class UIMainMenuCtrl : UIPanel
{
    public override UIDisplayMode DisplayMode => UIDisplayMode.FullScreen;

    [SerializeField]
    private Button m_CreateRoomBtn;
    [SerializeField]
    private Button m_JoinRoomBtn;
    [SerializeField]
    private Button m_CollectionBtn;
    [SerializeField]
    private Button m_SettingBtn;
    [SerializeField]
    private Button m_QuitGameBtn;

    private UIJoinDialogCtrl m_JoinCtrl;

    private void Awake()
    {
        UIUtils.ButtonBindListener(m_CreateRoomBtn,OnCreateRoomClick);
        UIUtils.ButtonBindListener(m_JoinRoomBtn,OnJoinRoomClick);
        UIUtils.ButtonBindListener(m_CollectionBtn,OnCollectionClick);
        UIUtils.ButtonBindListener(m_SettingBtn,OnSettingClick);
        UIUtils.ButtonBindListener(m_QuitGameBtn,OnQuitGameClick);
    }

    private void OnCreateRoomClick()
    {
        GameRoot.Instance.GameFlow.LobbySystem.CreateRoom(4);
    }

    private void OnJoinRoomClick()
    {
        m_JoinCtrl = UIMgr.Open<UIJoinDialogCtrl>();
    }

    private void OnCollectionClick()
    {
        
    }

    private void OnSettingClick()
    {
        
    }

    private void OnQuitGameClick()
    {
        Application.Quit();
    }

}