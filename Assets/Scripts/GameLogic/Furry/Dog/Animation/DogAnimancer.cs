using System.Collections;
using System.Collections.Generic;
using Animancer;
using UnityEngine;
using UnityEngine.AI;

public class DogAnimancer : MonoBehaviour
{
    [SerializeField] private AnimancerComponent animancer;
    [Header("移动混合")]
    [SerializeField] private LinearMixerTransition locomotionMixer;
    [Header("攻击")]
    [SerializeField] private ClipTransition attack;
    [Header("平滑")]
    [SerializeField, Min(0.1f)] private float parameterSmoothness = 8f;
    private NavMeshAgent agent;
    private float smoothedSpeed;
    private bool isPlayingAction;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    private void Update()
    {
        if (isPlayingAction || agent == null)
        {
            return;
        }
        UpdateLocomotion(agent.velocity.magnitude);
    }
    private void UpdateLocomotion(float speed)
    {
        if (locomotionMixer.State == null || !locomotionMixer.State.IsCurrent)
        {
            animancer.Play(locomotionMixer);
        }

        if (locomotionMixer.State == null)
        {
            return;
        }
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, speed, 1f - Mathf.Exp(-parameterSmoothness * Time.deltaTime));
        locomotionMixer.State.Parameter = smoothedSpeed;
    }
    public void PlayAttack()
    {
        AnimancerState state = animancer.Play(attack);
        isPlayingAction = true;
        // 攻击动画结束后回到移动混合
        state.Events(this).OnEnd = () => isPlayingAction = false;
    }
}
