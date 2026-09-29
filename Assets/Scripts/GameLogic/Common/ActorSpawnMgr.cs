using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ActorSpawnMgr : MonoBehaviour
{
    [SerializeField]
    private List<Transform> m_PlayersSpawnPos;
    [SerializeField]
    private List<Transform> m_EnemySpawnPos;


    public List<Transform> GetPlayerSpawnPositions()
    {
        return m_PlayersSpawnPos;
    }

    public List<Transform> GetEnemySpawnPositions()
    {
        return m_EnemySpawnPos;
    }
    
}
