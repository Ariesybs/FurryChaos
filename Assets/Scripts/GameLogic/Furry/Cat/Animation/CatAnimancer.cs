using System;
using System.Collections;
using System.Collections.Generic;
using Animancer;
using UnityEngine;
using Random = UnityEngine.Random;

public class CatAnimancer : MonoBehaviour
{
    [SerializeField] private AnimancerComponent animancer;
    [SerializeField] private CatAnimationProfile profile;
    [SerializeField] private MixerTransition2D crouchMixer;
    [SerializeField] private MixerTransition2D locomotionMixer;
    [SerializeField] private float smoothness = 10f;
    
    [HideInInspector]
    public Vector2 TargetParameter { get; private set; }    
    [HideInInspector]
    public Vector2 SmoothedParameter => smoothedParameter; // 实际喂给 Mixer 的值

    private AnimationClip currentClip;
    private AnimancerState currentState;
    private CatGait m_CurCatGait;
    private CatClip m_CurCatClip;
    private readonly Dictionary<string, CatClip> m_CatClipsDict = new();
    private Vector2 smoothedParameter;

    private void Awake()
    {
        if (profile != null)
        {
            m_CatClipsDict.Add("Idle",profile.Idle);
            m_CatClipsDict.Add("Walk",profile.WalkClip);
            m_CatClipsDict.Add("Crouch",profile.CrouchClip);
            m_CatClipsDict.Add("Run",profile.RunClip);
            m_CatClipsDict.Add("Jump",profile.JumpClip);
        }
    }

    public AnimancerState SwitchAnimation(string animClipKey)
    {
        if (m_CatClipsDict.TryGetValue(animClipKey, out var clip))
        {
            m_CurCatClip = clip;
            currentState = animancer.Play(clip.Clip,clip.FadeDuration);
            currentState.Speed = clip.Speed;
        }

        return currentState;
    }
    
    public void UpdateLocomotionVelocity(Vector3 worldVelocity, float deltaTime)
    {
        if (locomotionMixer.State == null || !locomotionMixer.State.IsCurrent)
        {
            animancer.Play(locomotionMixer);
        }

        if (locomotionMixer.State == null)
        {
            return;
        }
        
        CalculateAnimationParameter(worldVelocity, deltaTime);
        locomotionMixer.State.Parameter = smoothedParameter;
    }

    public void UpdateCrouchVelocity(Vector3 worldVelocity, float deltaTime)
    {
        if (crouchMixer.State == null || !crouchMixer.State.IsCurrent)
        {
            animancer.Play(crouchMixer);
        }

        if (crouchMixer.State == null)
        {
            return;
        }

        CalculateAnimationParameter(worldVelocity, deltaTime);
        crouchMixer.State.Parameter = smoothedParameter;
    }

    private void CalculateAnimationParameter(Vector3 worldVelocity, float deltaTime)
    {
        Vector3 planarVelocity = Vector3.ProjectOnPlane(worldVelocity, transform.up);
        Vector3 localVelocity = transform.InverseTransformDirection(planarVelocity);
        Vector2 targetParameter = new Vector2(localVelocity.x, localVelocity.z);
        TargetParameter = targetParameter; // 记录一下
        float t = 1f - Mathf.Exp(-smoothness * deltaTime);
        smoothedParameter = Vector2.Lerp(smoothedParameter, targetParameter, t);
    }

    private void MatchMovementAnimationSpeed(float moveSpeed)
    {
        if (locomotionMixer == null || locomotionMixer.State == null) return;
        // 不进行动画速度匹配
        if (!m_CurCatClip.MatchMovementSpeed)
        {
            locomotionMixer.State.Speed = m_CurCatClip.Speed;
            return;
        }
        var speedRatio = moveSpeed / m_CurCatClip.AuthoredMoveSpeed;
        locomotionMixer.State.Speed = m_CurCatClip.Speed * speedRatio;
    }

    public void PlaySequence(string[] keys)
    {
        var clips = new List<CatClip>();
        foreach (var key in keys)
        {
            if (m_CatClipsDict.TryGetValue(key, out var clip))
            {
                clips.Add(clip);
            }
        }
        PlaySequence(clips);
    }
    
    private void PlaySequence(List<CatClip> clips, int index = 0)
    {
        if (clips == null || index >= clips.Count)
        {
            return;
        }
        CatClip clip = clips[index];
        if (clip == null)
        {
            return;
        }
        AnimancerState state = animancer.Play(clip.Clip, clip.FadeDuration);
        state.Speed = clip.Speed;
        state.Events(this).OnEnd = () => PlaySequence(clips, index + 1);
    }

    private CatClip GetRandomClip(IList clipList)
    {
        if (clipList.Count == 0)
        {
            return null;
        }

        var idx = Random.Range(0, clipList.Count);
        return clipList[idx] as CatClip;
    }
}