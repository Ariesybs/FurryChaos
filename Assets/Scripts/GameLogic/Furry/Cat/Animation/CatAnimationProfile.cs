using UnityEngine;

[CreateAssetMenu(menuName = "Cat/Animation Profile")]
public sealed class CatAnimationProfile : ScriptableObject
{
    [Header("Idle")]
    public CatClip Idle;

    [Header("Walk")]
    public CatClip WalkClip;
    
    [Header("Run")]
    public CatClip RunClip;
    
    [Header("Jump")]
    public CatClip JumpClip;
    
    [Header("Crouch")]
    public CatClip CrouchClip;
    
    [Header("Sit")]
    public CatClip IdleToSit;
    public CatClip[] SitIdles;
    public CatClip SitEnd;
}