using System;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanelCtrl : MonoBehaviour
{
    public Button m_CreateRoomBtn;
    public Button m_JoinRoomBtn;

    void Start()
    {
        UIUtils.ButtonBindListener(m_CreateRoomBtn,OnCreateRoom);
        UIUtils.ButtonBindListener(m_JoinRoomBtn,OnJoinRoom);
    }

    private void OnCreateRoom()
    {
        StartHost();
    }

    private void OnJoinRoom()
    {
        StartClient();
    }

    private void StartHost()
    {
        GameRoot.Instance.GameNet.StartHost();
        GameRoot.Instance.GameNet.LoadScene(SceneDefine.GameScene);
    }

    private void StartClient()
    {
        GameRoot.Instance.GameNet.StartClient();
        GameRoot.Instance.GameNet.LoadScene(SceneDefine.GameScene);
    }
}
