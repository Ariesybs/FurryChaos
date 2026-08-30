using UnityEngine;

[CreateAssetMenu(menuName = "Cat/Movement Profile")]
public sealed class CatMovementProfile : ScriptableObject
{
    [Header("Movement")]
    public float CrouchSpeed = 1f;
    public float WalkSpeed = 2f;
    public float RunSpeed = 5f;

    [Header("Rotation")] 
    public float RotationSharpness = 12;
}