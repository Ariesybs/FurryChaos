using System;
using UnityEngine;

public class CatInput
{
    private bool m_LockInput;
    private bool m_LockCamera;
    private bool m_LockCursor;

    public void LockInput(bool _lock)
    {
        m_LockInput = _lock;
    }

    public void LockCamera(bool _lock)
    {
        m_LockCamera = _lock;
    }

    public void LockCursor(bool _lock)
    {
        Cursor.lockState = _lock ? CursorLockMode.Locked : CursorLockMode.None;
    }

    public InputCmd ReadCmd()
    {
        if (m_LockInput) return InputCmd.Empty;
        var cmd = new InputCmd()
        {
            Direction =  Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f)
        };
        var actions = InputAction.None;
        if (Input.GetKeyDown(KeyCode.Space))
            actions |= InputAction.Jump;
        if (Input.GetKeyDown(KeyCode.C))
            actions |= InputAction.Sit;
        if (Input.GetKeyDown(KeyCode.Z))
            actions |= InputAction.Lie;
        cmd.PressedActions = actions;
        return cmd;
    }
    
}

[Flags]
public enum InputAction : byte
{
    None = 0,
    Jump = 1 << 0,
    Sit  = 1 << 1,
    Lie  = 1 << 2,
}

public struct InputCmd
{
    public Vector2 Direction;
    public InputAction PressedActions;
    public static InputCmd Empty => default;
    
    public readonly bool IsPressed(InputAction action)
    {
        return (PressedActions & action) != 0;
    }

    public bool IsEmpty()
    {
        return Direction == Vector2.zero && PressedActions == InputAction.None;;
    }
}