using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.AI;

[TaskCategory("Dog/Movement")]
[TaskDescription("在出生点附近随机选择 NavMesh 位置巡逻。")]
public class RandomPatrol : Action
{
     public SharedFloat PatrolRadius = 8f;
    public SharedFloat ArrivalDistance = 0.3f;
    public SharedFloat WaitDuration = 1.5f;
    private NavMeshAgent agent;
    private Vector3 patrolCenter;
    private bool hasDestination;
    private bool isWaiting;
    private float waitTimer;
    public override void OnAwake()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolCenter = transform.position;
    }
    
    public override void OnStart()
    {
        isWaiting = false;
        waitTimer = 0f;
        hasDestination = TrySetNextDestination();
    }
    public override TaskStatus OnUpdate()
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            return TaskStatus.Failure;
        }
        if (!hasDestination)
        {
            hasDestination = TrySetNextDestination();
            return hasDestination ? TaskStatus.Running : TaskStatus.Failure;
        }
        if (agent.pathPending)
        {
            return TaskStatus.Running;
        }
        // 当前随机点无法到达，重新选一个。
        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            hasDestination = TrySetNextDestination();
            return TaskStatus.Running;
        }
        bool arrived = agent.remainingDistance <= ArrivalDistance.Value && agent.velocity.sqrMagnitude < 0.05f;
        if (!arrived)
        {
            return TaskStatus.Running;
        }
        if (!isWaiting)
        {
            isWaiting = true;
            waitTimer = 0f;
            agent.ResetPath();
        }
        waitTimer += Time.deltaTime;
        if (waitTimer >= WaitDuration.Value)
        {
            isWaiting = false;
            hasDestination = TrySetNextDestination();
        }
        // 巡逻节点持续运行，直到被发现猫的条件中断。
        return TaskStatus.Running;
    }
    private bool TrySetNextDestination()
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            return false;
        }
        const int maxAttempts = 10;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * PatrolRadius.Value;
            Vector3 randomPosition = patrolCenter + transform.right * randomCircle.x + transform.forward * randomCircle.y;
            if (!NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 2f, agent.areaMask))
            {
                continue;
            }
            NavMeshPath path = new NavMeshPath();
            if (!agent.CalculatePath(hit.position, path) ||
                path.status != NavMeshPathStatus.PathComplete)
            {
                continue;
            }
            agent.stoppingDistance = ArrivalDistance.Value;
            return agent.SetDestination(hit.position);
        }
        return false;
    }
    public override void OnEnd()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
        hasDestination = false;
        isWaiting = false;
    }
    public override void OnReset()
    {
        PatrolRadius = 8f;
        ArrivalDistance = 0.3f;
        WaitDuration = 1.5f;
    }
}
