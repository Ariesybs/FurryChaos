using System;
using UnityEngine;
using UnityEngine.UI;

public class UIBagItemCtrl : MonoBehaviour
{
    public Image icon;

    private void Awake()
    {
        UIUtils.SetActive(icon,false);
    }

    public void Setup(uint itemId)
    {
        UIUtils.SetActive(icon,true);
    }

    public void Hide()
    {
        UIUtils.SetActive(icon,false);
    }
}