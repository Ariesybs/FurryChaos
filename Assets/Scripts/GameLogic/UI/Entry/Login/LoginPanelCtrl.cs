using System;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanelCtrl : MonoBehaviour
{
    public Button m_CreateRoomBtn;
    public Button m_JoinRoomBtn;
    public Button m_OpenPanelBtn;
    public Button m_LeaveRoomBtn;
    public Button m_QuitGameBtn;
    public ulong roomId;

    private LobbySystem lobby => GameRoot.Instance.GameFlow.LobbySystem;
    void Start()
    {
        UIUtils.ButtonBindListener(m_CreateRoomBtn,OnCreateRoom);
        UIUtils.ButtonBindListener(m_JoinRoomBtn,OnJoinRoom);
        UIUtils.ButtonBindListener(m_OpenPanelBtn,OnOpenPanel);
        UIUtils.ButtonBindListener(m_LeaveRoomBtn, OnLeaveRoom);
        UIUtils.ButtonBindListener(m_QuitGameBtn,OnQuitGame);
    }

    private void OnCreateRoom()
    {
        lobby.CreateRoom();
    }

    private void OnJoinRoom()
    {
        lobby.JoinRoom(roomId);
    }

    private void OnOpenPanel()
    {
        SteamSdk.OpenInviteDialog(lobby.CurrentLobbyId);
    }

    private void OnLeaveRoom()
    {
        GameRoot.Instance.GameFlow.LobbySystem.LeaveRoom();
    }

    private void OnQuitGame()
    {
        Application.Quit();
    }
    
}
