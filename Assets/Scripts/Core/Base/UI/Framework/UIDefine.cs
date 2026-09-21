public interface IUIArgs
{
}

public readonly struct UIEmptyArgs : IUIArgs
{
    public static readonly UIEmptyArgs Default;
}

public enum UIDisplayMode
{
    FullScreen,
    Popup
}
