using System;

public class UISettingCtrl : UIPanel
{
    public override UIDisplayMode DisplayMode => UIDisplayMode.FullScreen;

    private void OnEnable()
    {
        GameInput.AddListener(OnActionInput);
    }

    private void OnActionInput(GameAction action)
    {
        if (action == GameAction.Pause)
        {
            CloseSelf();
        }
    }

    private void OnDisable()
    {
        GameInput.RemoveListener(OnActionInput);
    }
}