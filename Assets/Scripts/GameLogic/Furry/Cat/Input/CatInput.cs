using System;
using Unity.Netcode;
using UnityEngine;

public class CatInput
{
    private bool m_LockInput;
    private bool m_LockCamera;
    private bool m_LockCursor;

    public void LockInput(bool _lock)
    {
        // Debug.Log($"Lock Input: {_lock}");
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

    public bool IsCursorLocked()
    {
        return Cursor.lockState == CursorLockMode.Locked;
    }

    public InputCmd ReadCmd()
    {
        var input = GameRoot.Instance.GameInput;
        if (m_LockInput || input == null || !input.GameplayEnabled) return InputCmd.Empty;
        var cmd = new InputCmd()
        {
            Direction = input.ReadMoveAxis()
        };
        var pressedActions = InputAction.None;
        var heldActions = InputAction.None;

        if (input.GetDown(GameAction.Jump)) pressedActions |= InputAction.Jump;
        if (input.GetDown(GameAction.Sit)) pressedActions |= InputAction.Sit;
        if (input.GetDown(GameAction.Lie)) pressedActions |= InputAction.Lie;
        if (input.GetHeld(GameAction.Crouch)) heldActions |= InputAction.Crouch;
        if (input.GetHeld(GameAction.Run)) heldActions |= InputAction.Run;

        cmd.PressedActions = pressedActions;
        cmd.HeldActions = heldActions;
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
    Crouch = 1 << 3,
    Run = 1 << 4,
}

public struct InputCmd : INetworkSerializable
{
    public uint Sequence;
    public uint ClientTick;
    public Vector2 Direction;
    public float CameraYaw;
    public InputAction PressedActions;
    public InputAction HeldActions;
    public static InputCmd Empty => default;
    
    public readonly bool IsPressed(InputAction action)
    {
        return (PressedActions & action) != 0;
    }

    public readonly bool IsHeld(InputAction action)
    {
        return (HeldActions & action) != 0;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref Sequence);
        serializer.SerializeValue(ref ClientTick);
        serializer.SerializeValue(ref Direction);
        serializer.SerializeValue(ref CameraYaw);

        byte pressedActions = (byte)PressedActions;
        byte heldActions = (byte)HeldActions;
        serializer.SerializeValue(ref pressedActions);
        serializer.SerializeValue(ref heldActions);

        if (serializer.IsReader)
        {
            PressedActions = (InputAction)pressedActions;
            HeldActions = (InputAction)heldActions;
        }
    }

    public bool IsEmpty()
    {
        return Direction == Vector2.zero &&
               PressedActions == InputAction.None &&
               HeldActions == InputAction.None;
    }
}