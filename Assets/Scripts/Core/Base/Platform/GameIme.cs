using System;
using System.Runtime.InteropServices;

public static class GameIme
{
    private const uint Activate = 0x00000001;
    private const uint SetForProcess = 0x00000100;

    [DllImport("user32.dll")]
    private static extern IntPtr GetKeyboardLayout(uint idThread);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint flags);

    [DllImport("user32.dll")]
    private static extern IntPtr ActivateKeyboardLayout(IntPtr hkl, uint flags);

    private static IntPtr s_Previous;

    public static void Disable()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        if (s_Previous == IntPtr.Zero)
        {
            s_Previous = GetKeyboardLayout(0);
        }

        LoadKeyboardLayout("00000409", Activate | SetForProcess);
#endif
    }

    public static void Restore()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        if (s_Previous == IntPtr.Zero)
        {
            return;
        }

        ActivateKeyboardLayout(s_Previous, 0);
        s_Previous = IntPtr.Zero;
#endif
    }
}