using System;
using UnityEngine;

/// <summary>
/// 交互物探测器
/// </summary>
public class InteractionDetector : MonoBehaviour
{
    [SerializeField]
    private float m_Radius = 2f;
    [SerializeField]
    private LayerMask m_DetectorLayer;

    private readonly Collider[] m_Hits = new Collider[8];
    public IInteractable Current { get; private set; }
    private ulong m_LocalPlayerNetId;

    private void Awake()
    {
#if LOCAL_DEBUG
        m_LocalPlayerNetId = 0;
#else
        m_LocalPlayerNetId = GameRoot.Instance.GameNet.CurNetId;
#endif
    }

    private void FixedUpdate()
    {
        IInteractable now = null;
        var hitCount = Physics.OverlapSphereNonAlloc(transform.position, m_Radius, m_Hits, m_DetectorLayer);
        var bestDistance = float.PositiveInfinity;
        for (var i = 0; i < hitCount; i++)
        {
            var hit = m_Hits[i];
            var interactableObj = hit.GetComponentInParent<IInteractable>();
            if (interactableObj == null || !interactableObj.CanInteract(m_LocalPlayerNetId))
            {
                continue;
            }

            var distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < bestDistance)
            {
                now = interactableObj;
                bestDistance = distance;
            }
        }

        if (now != Current)
        {
            Current = now;
#if LOCAL_DEBUG
            Debug.Log(Current != null ? Current.GetPrompt() : "NULL");
#else
            Log.Info("InteractionDetector",Current != null ? Current.GetPrompt() : "NULL");
#endif
            // 更新UI
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position,m_Radius);
    }
}