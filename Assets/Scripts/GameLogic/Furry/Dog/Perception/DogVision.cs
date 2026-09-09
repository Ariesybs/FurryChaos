using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DOG的视觉组件
public class DogVision : MonoBehaviour
{
    [SerializeField] private Transform eyePoint;
    [SerializeField, Min(0f)] private float viewDistance = 10f;
    [SerializeField, Range(0f, 360f)] private float viewAngle = 120f;
    [SerializeField] private LayerMask catMask;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField, Min(1)] private int maxResults = 16;
    
    private Collider[] results;
    
    private void Awake()
    {
        results = new Collider[maxResults];
    }

    public bool TryFindVisibleCat(out GameObject cat)
    {
        cat = null;
        var count = Physics.OverlapSphereNonAlloc(eyePoint.position, viewDistance, results, catMask, QueryTriggerInteraction.Ignore);
        var bestDistanceSqr = float.PositiveInfinity;
        for (int i = 0; i < count; i++)
        {
            var candidate = results[i].GetComponentInParent<CatCharacter>();
            if(candidate == null) continue;
            if(!CanSeeCat(candidate.gameObject))
            {
                continue;
            }
            float distanceSqr = (candidate.transform.position - transform.position).sqrMagnitude;
            if (distanceSqr >= bestDistanceSqr)
            {
                // 非最近单位
                continue;
            }
            bestDistanceSqr = distanceSqr; // 更新最近单位
            cat = candidate.gameObject;
        }
        return cat != null;
    }

    public bool CanSeeCat(GameObject target)
    {
        if (target == null) return false;

        var distance = (target.transform.position - transform.position).magnitude;
        if (distance <= 0.001f || distance > viewDistance)
        {
            // 不在视野范围
            return false;
        }

        var toTarget = target.transform.position - transform.position;
        var angle = Vector3.Angle(transform.forward, toTarget); // 视锥
        if (angle > viewAngle * 0.5f)
        {
            // 非视锥范围
            return false;
        }
        
        var isBlocked = Physics.Raycast(transform.position, toTarget , distance, obstacleMask, QueryTriggerInteraction.Ignore);

        return !isBlocked;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (eyePoint == null)
        {
            return;
        }
        Vector3 origin = eyePoint.position;
        Vector3 up = transform.up;
        Vector3 forward = transform.forward;
        float halfAngle = viewAngle * 0.5f;
        // 视锥左右边界
        Vector3 leftDir = Quaternion.AngleAxis(-halfAngle, up) * forward;
        Vector3 rightDir = Quaternion.AngleAxis(halfAngle, up) * forward;
        Gizmos.color = new Color(1f, 0.92f, 0.16f, 0.9f);
        Gizmos.DrawLine(origin, origin + leftDir * viewDistance);
        Gizmos.DrawLine(origin, origin + rightDir * viewDistance);
        // 画扇形弧
        const int segmentCount = 32;
        Vector3 prevPoint = origin + leftDir * viewDistance;
        for (int i = 1; i <= segmentCount; i++)
        {
            float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segmentCount);
            Vector3 dir = Quaternion.AngleAxis(angle, up) * forward;
            Vector3 point = origin + dir * viewDistance;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
}
