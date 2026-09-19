using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class UIHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float m_HoverScale = 1.15f;
    [SerializeField] private float m_Duration = 0.18f;
    [SerializeField] private Ease m_Ease = Ease.OutQuad;

    private Vector3 m_OriginScale;
    private Tween m_Tween;

    private void Awake()
    {
        m_OriginScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayScale(m_OriginScale * m_HoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PlayScale(m_OriginScale);
    }

    private void PlayScale(Vector3 target)
    {
        // 打断上一段，避免快速进出时打架
        m_Tween.Stop();
        float full = Mathf.Abs(m_OriginScale.x * m_HoverScale - m_OriginScale.x);
        float remain = Mathf.Abs(transform.localScale.x - target.x);
        float duration = full > 0.0001f ? m_Duration * (remain / full) : 0f;
        m_Tween = Tween.Scale(transform, target, duration, Ease.OutQuad);
    }

    private void OnDisable()
    {
        m_Tween.Stop();
        transform.localScale = m_OriginScale;
    }
}