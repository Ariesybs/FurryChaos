using UnityEngine;

[CreateAssetMenu(menuName = "Cat/Animation Profile")]
public sealed class CatAnimationProfile : ScriptableObject
{
    [Header("Idle")]
    public CatClip[] Idles;
    public CatClip SitToIdle;

    [Header("Walk")]
    public CatClip WalkClip;
    
    [Header("Run")]
    public CatClip Run;
    
    [Header("Jump")]
    public CatClip Jump;
    
    [Header("Sit")]
    public CatClip IdleToSit;
    public CatClip[] SitIdles;
    public CatClip SitEnd;
}