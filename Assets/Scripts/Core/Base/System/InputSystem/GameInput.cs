using System;

public static class GameInput
{
    public static void AddListener(Action<GameAction> action)
    {
        GameRoot.Instance.GameInput.ActionDown += action;
    }

    public static void RemoveListener(Action<GameAction> action)
    {
        GameRoot.Instance.GameInput.ActionDown -= action;
    }
}