using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICharacterPreviewCtrl : MonoBehaviour
{
    [SerializeField]
    private List<UIAvatarItemCtrl> itemList;

    private UIAvatarItemCtrl curSelectedItem;
    void Start()
    {
        foreach (var itemCtrl in itemList)
        {
            itemCtrl?.SetSelectCallback(OnItemSelected);
        }

        if (itemList.Count > 0)
        {
            OnItemSelected(itemList[0]);// 默认选中第一个
        }
    }

    private void OnItemSelected(UIAvatarItemCtrl item)
    {
        curSelectedItem = item;
        foreach (var itemCtrl in itemList)
        {
            itemCtrl?.OnSelect(curSelectedItem);
        }
    }
}
