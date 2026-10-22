using BehaviorDesigner.Runtime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class DogCharacter : NetworkBehaviour
{
    private BehaviorTree m_BehaviorTree;
    private NavMeshAgent m_Agent;
    private readonly NetworkVariable<float> m_Speed = new();
    public bool debug;
    public float SyncedSpeed => m_Speed.Value;

    private void Awake()
    {
        m_BehaviorTree = GetComponent<BehaviorTree>();
        m_Agent = GetComponent<NavMeshAgent>();
        if (debug)
        {
            m_BehaviorTree.EnableBehavior();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            m_BehaviorTree.EnableBehavior();
            return;
        }

        m_Agent.enabled = false;
    }

    private void Update()
    {
        if (!IsServer || m_Agent == null)
        {
            return;
        }

        if (m_Speed != null)
        {
            m_Speed.Value = m_Agent.velocity.magnitude;
        }
    }
}