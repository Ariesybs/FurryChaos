using System;
using UnityEngine;

[DisallowMultipleComponent]
public class CatJumpScanner : MonoBehaviour
{
    private CatCharacter cat;
    [Header("扫描配置")]
    [SerializeField] private LayerMask jumpLinkMask;
    [SerializeField, Min(0.1f)] private float scanRadius = 3f;
    [SerializeField, Min(1)] private int maxResults = 16;
    [Header("候选评分")]
    [SerializeField] private float directionWeight = 2f;
    [SerializeField] private float distanceWeight = 1f;
    [SerializeField] private float heightWeight = 0.5f;
    
    private Collider[] m_Results;
    public CatJumpLink CurrentLink { get; private set; }

    private void Awake()
    {
        cat = GetComponent<CatCharacter>();
        m_Results = new Collider[maxResults];
    }

    public bool TryFindBestJumpLink(Vector3 desiredDirection, out CatJumpLink bestLink)
    {
        bestLink = null;
        CurrentLink = null;
        if (cat == null || cat.motor == null) return false;
        
        Vector3 characterPosition = cat.motor.TransientPosition;
        Vector3 characterUp = cat.motor.CharacterUp;
        Vector3 searchDirection = Vector3.ProjectOnPlane(desiredDirection, characterUp);
        // 没有移动输入时，使用猫当前朝向。
        if (searchDirection.sqrMagnitude < 0.001f)
        {
            searchDirection = Vector3.ProjectOnPlane(cat.motor.TransientRotation * Vector3.forward, characterUp);
        }
        searchDirection.Normalize();
        var count = Physics.OverlapSphereNonAlloc(characterPosition, scanRadius, m_Results, jumpLinkMask, QueryTriggerInteraction.Collide);
        var bestScore = float.NegativeInfinity;
        for (int i = 0; i < count; i++)
        {
            Collider candidateCollider = m_Results[i];
            if (candidateCollider == null)
                continue;
            CatJumpLink link = candidateCollider.GetComponent<CatJumpLink>();
            if (link == null) continue;
            
            if (!link.IsInActivationRange(characterPosition, characterUp))
            {
                // 不在范围
                continue;
            }
            if (!link.MatchesApproachDirection(characterPosition, searchDirection, characterUp))
            {
                // 方向非法
                continue;
            }
            float score = CalculateScore(link, characterPosition, characterUp, searchDirection);
            
            if (score <= bestScore) continue;
            bestScore = score;
            bestLink = link;
        }
        CurrentLink = bestLink;
        return bestLink != null;
    }
    
    private float CalculateScore(CatJumpLink link, Vector3 characterPosition, Vector3 characterUp, Vector3 desiredDirection)
    {
        Vector3 toLanding = link.transform.position - characterPosition;
        float heightDifference = Mathf.Abs(Vector3.Dot(toLanding, characterUp));
        Vector3 planarToLanding = Vector3.ProjectOnPlane(toLanding, characterUp);
        float distance = planarToLanding.magnitude;
        float directionScore = 0f;
        if (planarToLanding.sqrMagnitude > 0.001f)
        {
            directionScore = Vector3.Dot(desiredDirection, planarToLanding.normalized);
        }
        return directionScore * directionWeight - distance * distanceWeight - heightDifference * heightWeight;
    }
    
    public void ClearCurrentLink()
    {
        CurrentLink = null;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = CurrentLink != null ? Color.green : Color.yellow;
        Vector3 position = cat != null && cat.motor != null ? cat.motor.TransientPosition : transform.position;
        Gizmos.DrawWireSphere(position, scanRadius);
        if (CurrentLink == null || CurrentLink.transform == null)
        {
            return;
        }
        Gizmos.color = Color.green;
        Gizmos.DrawLine(position, CurrentLink.transform.position);
    }
}