using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAvatarItemCtrl : MonoBehaviour
{
    [SerializeField]
    private Button itemBtn;
    [SerializeField]
    private GameObject selectedGo;

    private Action<UIAvatarItemCtrl> onSelectedCallback;
    void Start()
    {
        UIUtils.ButtonBindListener(itemBtn,OnItemBtnClick);
    }

    private void OnItemBtnClick()
    {
        onSelectedCallback?.Invoke(this);
    }

    public void SetSelectCallback(Action<UIAvatarItemCtrl> callback)
    {
        onSelectedCallback = callback;
    }

    public void OnSelect(UIAvatarItemCtrl item)
    {
        UIUtils.SetActive(selectedGo,item == this);
    }
}
