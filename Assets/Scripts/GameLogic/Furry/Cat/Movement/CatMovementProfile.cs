using UnityEngine;

[CreateAssetMenu(menuName = "Cat/Movement Profile")]
public sealed class CatMovementProfile : ScriptableObject
{
    [Header("Movement")]
    public float MoveSpeed = 3.36f;
    public float GroundSharpness = 15f;
    public float AirAcceleration = 8f;

    [Header("Turning")]
    public float RotationSharpness = 12f;
    [Range(-1f, 1f)] public float ForwardCosThreshold = 0.7f;
    [Range(0f, 1f)] public float MinTurnSpeedMultiplier = 0.25f;

    [Header("Gravity")]
    public Vector3 Gravity = new(0, -25f, 0);
}