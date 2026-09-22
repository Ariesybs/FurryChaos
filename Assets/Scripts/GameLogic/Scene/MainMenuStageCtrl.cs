using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class MainMenuStageCtrl : MonoBehaviour
{
    public enum TurnTo
    {
        Customization,
        MainMenu,
        StartGame
    }
    public Camera mainCamera;
    void Start()
    {
        GameEvent.Register<UIMainMenuSelectOptMsg>(OnMainMenuSelect);
    }

    private void OnMainMenuSelect(UIMainMenuSelectOptMsg msg)
    {
        switch (msg.type)
        {
            case UIMainMenuSelectOptMsg.OptType.Idle:
                RotateCamera(TurnTo.MainMenu);
                break;
            case UIMainMenuSelectOptMsg.OptType.Customization:
                RotateCamera(TurnTo.Customization);
                break;
        }
    }

    public void RotateCamera(TurnTo to)
    {
        if (mainCamera == null)
        {
            return;
        }
        var angleY = to switch
        {
            TurnTo.Customization => -90,
            TurnTo.MainMenu => 0,
            TurnTo.StartGame => 90,
            _ => 0
        };
        Tween.LocalRotation(mainCamera.transform, new Vector3(0f, angleY, 0f), 1.5f);
    }
}
