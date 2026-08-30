using UnityEngine;

[CreateAssetMenu(menuName = "Cat/Movement Profile")]
public sealed class CatMovementProfile : ScriptableObject
{
    [Header("Movement")]
    public float CrouchSpeed = 1f;
    public float WalkSpeed = 2f;
    public float RunSpeed = 5f;
    
    [Header("Jump")]
    public float JumpChargeTime = 1f;

    [Header("Rotation")] 
    public float RotationSharpness = 12;
    
    [Header("Gravity")]
    public Vector3 Gravity = new(0f, -25f, 0f);
}