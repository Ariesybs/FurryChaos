using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.AI;

[TaskCategory("Dog/Movement")]
[TaskDescription("追踪目标，进入攻击距离后返回成功。")]
public sealed class ChaseCat : Action
{
    [SharedRequired]
    public SharedGameObject Target;
    public SharedFloat AttackDistance = 1.2f;
    private NavMeshAgent agent;
    
    public override void OnAwake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public override TaskStatus OnUpdate()
    {
        if (agent == null || !agent.isOnNavMesh || Target.Value == null)
        {
            return TaskStatus.Failure;
        }
        var targetPosition = Target.Value.transform.position;
        agent.stoppingDistance = AttackDistance.Value; // 进入攻击范围后停止移动
        if (!agent.SetDestination(targetPosition))
        {
            // 目标点设置失败
            return TaskStatus.Failure;
        }

        if (agent.pathPending)
        {
            // 正在规划路线
            return TaskStatus.Running;
        }

        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            return TaskStatus.Failure;
        }
        
        if (agent.remainingDistance <= AttackDistance.Value)
        {
            // 到达攻击范围
            agent.ResetPath();
            return TaskStatus.Success;
        }
        
        return TaskStatus.Running;
    }
    
    public override void OnEnd()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }
    
    public override void OnReset()
    {
        Target = null;
        AttackDistance = 1.2f;
    }
}
