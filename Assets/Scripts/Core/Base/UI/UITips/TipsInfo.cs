using UnityEngine;

public struct TipsInfo
{
    public int TipId;              // 业务方自定义的稳定 ID，同 ID 重复 Show 视为更新
    public ENUM_TIPS_TYPE TipsType;// 提示类型
    public TipAnchor Anchor;       // 世界锚点：固定点 或 跟随 Transform（引导狗/路点会动）
    public string Text;            // “跳上桌面” / “去厨房”
    public string KeyHint;         // “空格” / “E”，没有则只显示文字
    public Sprite Icon;            // 提示图标
    public int Priority;           // 同屏超出上限时按优先级裁剪
    public Transform Observer;
    public float ShowDistance;     // 进入该距离才显示（跳跃提示用）
    public bool HideWhenOccluded;  // 被墙挡住时隐藏（跳跃=true，引导=false）
    public bool ClampToScreenEdge; // 目标在屏幕外时钉在边缘+箭头（引导=true，跳跃=false）
    public float Duration;         // ≤0 表示由业务方手动 Hide（引导/跳跃都是）

    public Vector3 GetPos()
    {
        return this.Anchor.Follow != null ? this.Anchor.Follow.position + this.Anchor.Offset : this.Anchor.FixedPoint + this.Anchor.Offset;
    }
}

// 锚点：固定点与跟随目标二选一
public struct TipAnchor
{
    public Vector3 FixedPoint;      // 固定点，引导用
    public Transform Follow;        // 跟随目标，引导用
    public Vector3 Offset;         // 头顶偏移之类的微调
}

public enum ENUM_TIPS_TYPE
{
    TIPS_LANDING_SURFACE
}