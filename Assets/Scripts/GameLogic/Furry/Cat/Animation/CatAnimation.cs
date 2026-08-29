using System;
using UnityEngine;

[Serializable]
public sealed class CatClip
{
    public AnimationClip Clip;
    public float FadeDuration = 0.15f;
    public float Speed = 1f;
    [Header("是否动画播放速度匹配移动速度")]
    public bool MatchMovementSpeed;
    [Header("动画以 1 倍速播放时对应的移动速度")]
    public float AuthoredMoveSpeed = 1f;
}
