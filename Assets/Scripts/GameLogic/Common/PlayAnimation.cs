using System;
using System.Collections;
using System.Collections.Generic;
using Animancer;
using UnityEngine;

public class PlayAnimation : MonoBehaviour
{
    public AnimancerComponent animancer;
    public AnimationClip clip;
    void Start()
    {
        if (animancer != null && clip != null)
        {
            animancer.Play(clip);
        }
    }

    private void Reset()
    {
        animancer = GetComponent<AnimancerComponent>();
    }
}
