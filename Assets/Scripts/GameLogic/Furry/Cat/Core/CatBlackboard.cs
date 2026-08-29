using UnityEngine;

public enum CatBehavior
{
    Idle,
    Forward,
    TurnLeft,
    TurnRight
}

public struct CatMovementIntent
{
    public Vector2 RawMove;
    public Vector3 WorldMoveDirection;
}

public struct CatMotorSnapshot
{
    public CatBehavior Behavior;
    public Vector3 Velocity;
    public float PlanarSpeed;
    public float DirectionCosine;
    public float SpeedMultiplier;
    public bool IsGrounded;
}

public sealed class CatBlackboard
{
    // 输入层写，KCC 读。
    public CatMovementIntent Intent;
    // KCC 写，动画和调试面板读。
    public CatMotorSnapshot Motion;
}