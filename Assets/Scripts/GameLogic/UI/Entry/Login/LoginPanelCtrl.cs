using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanelCtrl : MonoBehaviour
{
    public Button m_LoginBtn;
    void Start()
    {
        if (m_LoginBtn != null)
        {
            m_LoginBtn.onClick.AddListener(OnLoginBtnClick);
        }
    }

    private void OnLoginBtnClick()
    {
        var msg = NetworkMsg.Get<C2S_JoinRequest>();
        GameNet.Send(msg);
    }
    
}
