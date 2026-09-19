using UnityEngine;

public class ActorRotate : MonoBehaviour
{
    [Header("旋转目标")]
    public Transform ActorContainer;

    [Header("输入")]
    [SerializeField] private Camera m_Camera;
    [SerializeField] private LayerMask m_HitMask = ~0;
    [SerializeField] private float m_RotateSpeed = 0.35f;
    [SerializeField] private bool m_InvertX;

    [Header("阻尼 / 平滑")]
    [SerializeField, Min(0.01f)] private float m_Acceleration = 12f;
    [SerializeField, Min(0.01f)] private float m_Damping = 8f;
    [SerializeField, Min(0f)] private float m_MaxAngularSpeed = 360f;
    [SerializeField, Min(0f)] private float m_StopSpeed = 0.5f;

    private bool m_Dragging;
    private float m_LastMouseX;
    private float m_AngularVelocity;
    private Collider m_TriggerCollider;

    private void Awake()
    {
        if (m_Camera == null)
            m_Camera = Camera.main;

        if (ActorContainer == null)
            ActorContainer = transform;

        m_TriggerCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (m_Camera == null || ActorContainer == null)
            return;

        if (Input.GetMouseButtonDown(0) && TryHitSelf(out _))
        {
            m_Dragging = true;
            m_LastMouseX = Input.mousePosition.x;
        }

        if (Input.GetMouseButtonUp(0))
            m_Dragging = false;

        float targetVelocity = 0f;

        if (m_Dragging && Input.GetMouseButton(0))
        {
            float mouseX = Input.mousePosition.x;
            float deltaX = mouseX - m_LastMouseX;
            m_LastMouseX = mouseX;

            // 像素位移 -> 目标角速度
            targetVelocity = deltaX / Mathf.Max(Time.deltaTime, 0.0001f) * m_RotateSpeed;
            if (m_InvertX)
                targetVelocity = -targetVelocity;
        }

        // 逼近目标角速度（拖拽时跟随，松开后目标为0）
        m_AngularVelocity = Mathf.Lerp(
            m_AngularVelocity,
            targetVelocity,
            1f - Mathf.Exp(-m_Acceleration * Time.deltaTime));

        // 额外阻尼，松手后慢慢停
        if (!m_Dragging)
        {
            m_AngularVelocity = Mathf.Lerp(
                m_AngularVelocity,
                0f,
                1f - Mathf.Exp(-m_Damping * Time.deltaTime));
        }

        m_AngularVelocity = Mathf.Clamp(
            m_AngularVelocity,
            -m_MaxAngularSpeed,
            m_MaxAngularSpeed);

        if (Mathf.Abs(m_AngularVelocity) < m_StopSpeed && !m_Dragging)
        {
            m_AngularVelocity = 0f;
            return;
        }

        ActorContainer.Rotate(
            0f,
            m_AngularVelocity * Time.deltaTime,
            0f,
            Space.World);
    }

    private bool TryHitSelf(out RaycastHit hit)
    {
        Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(
                ray,
                out hit,
                1000f,
                m_HitMask,
                QueryTriggerInteraction.Collide))
        {
            return false;
        }

        return hit.collider.transform == transform ||
               hit.collider.transform.IsChildOf(transform) ||
               (m_TriggerCollider != null && hit.collider == m_TriggerCollider);
    }
}