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

    private AnimationClip currentClip;
    private AnimancerState currentState;
    private CatGait m_CurCatGait;
    private CatClip m_CurCatClip;
    private Dictionary<string, CatClip> m_CatClipsDict = new();

    private void Awake()
    {
        if (profile != null)
        {
            m_CatClipsDict.Add("Idle",GetRandomClip(profile.Idles));
            m_CatClipsDict.Add("Sit_To_Idle",profile.SitToIdle);
            m_CatClipsDict.Add("Walk",profile.WalkClip);
            m_CatClipsDict.Add("Crouch",profile.CrouchClip);
            m_CatClipsDict.Add("Run",profile.RunClip);
            m_CatClipsDict.Add("Idle_To_Sit",profile.IdleToSit);
            m_CatClipsDict.Add("Sit_Idle",GetRandomClip(profile.SitIdles));
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
    
    public void UpdateLocomotion(CatGait gait, float moveSpeed)
    {
        if (gait != m_CurCatGait)
        {
            m_CurCatGait = gait;
            switch (gait)
            {
                case CatGait.Idle:
                    SwitchAnimation("Idle");
                    break;
                case CatGait.Walk:
                    SwitchAnimation("Walk");
                    break;
                case CatGait.Run:
                    SwitchAnimation("Run");
                    break;
                case CatGait.Crouch:
                    SwitchAnimation("Crouch");
                    break;
            }
        }
        MatchMovementAnimationSpeed(moveSpeed);
    }

    private void MatchMovementAnimationSpeed(float moveSpeed)
    {
        if (currentState == null || m_CurCatClip == null) return;
        // 不进行动画速度匹配
        if (!m_CurCatClip.MatchMovementSpeed)
        {
            currentState.Speed = m_CurCatClip.Speed;
            return;
        }
        var speedRatio = moveSpeed / m_CurCatClip.AuthoredMoveSpeed;
        currentState.Speed = m_CurCatClip.Speed * speedRatio;
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