using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogCombat : MonoBehaviour
{
    [SerializeField, Min(0f)] private float attackDistance = 1.2f;
    [SerializeField, Min(0f)] private float attackDuration = 0.8f;
    [SerializeField, Min(0f)] private float hitTime = 0.35f;
    [SerializeField, Min(0f)] private float cooldown = 1f;
    
    private float nextAttackTime;
    public float AttackDuration => attackDuration;
    public float HitTime => hitTime;
    private DogAnimancer animancer;
    
    private void Awake()
    {
        animancer = GetComponent<DogAnimancer>();
    }
    public bool CanBeginAttack(GameObject target)
    {
        if (target == null || Time.time < nextAttackTime)
        {
            // 攻击间隔未到
            return false;
        }
        float distanceSqr = (target.transform.position - transform.position).sqrMagnitude;
        // 距离过远
        return distanceSqr <= attackDistance * attackDistance;
    }
    
    public void BeginAttack(GameObject target)
    {
        nextAttackTime = Time.time + cooldown;
        animancer?.PlayAttack();
    }

    public void ApplyHit(GameObject target)
    {
        Debug.Log("Attack Cat!");
    }
}
