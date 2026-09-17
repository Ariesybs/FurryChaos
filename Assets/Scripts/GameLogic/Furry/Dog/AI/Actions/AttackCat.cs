using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Dog/Combat")]
[TaskDescription("面向目标执行一次攻击。")]
public class AttackCat : Action
{
    [SharedRequired]
    public SharedGameObject Target;
    private DogCombat combat;
    private float elapsed;
    private bool started;
    private bool hitApplied;
    
    public override void OnAwake()
    {
        combat = GetComponent<DogCombat>();
    }

    public override void OnStart()
    {
        elapsed = 0f;
        hitApplied = false;
        started = combat != null && combat.CanBeginAttack(Target.Value);
        if (started)
        {
            combat.BeginAttack(Target.Value);
        }
    }
    
    public override TaskStatus OnUpdate()
    {
        if (!started || Target.Value == null)
        {
            return TaskStatus.Failure;
        }
        Vector3 direction = Target.Value.transform.position - transform.position;
        direction = Vector3.ProjectOnPlane(direction, transform.up);
        if (direction.sqrMagnitude > 0.001f)
        {
            // 朝向目标
            Quaternion targetRotation = Quaternion.LookRotation(direction, transform.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - Mathf.Exp(-12f * Time.deltaTime));
        }
        elapsed += Time.deltaTime;
        if (!hitApplied && elapsed >= combat.HitTime)
        {
            hitApplied = true;
            combat.ApplyHit(Target.Value);
        }
        return elapsed >= combat.AttackDuration ? TaskStatus.Success : TaskStatus.Running;
    }
    
    public override void OnReset()
    {
        Target = null;
    }
}
