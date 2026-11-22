using UnityEngine;

/// <summary>
/// 可站立表面扫描器：挂在猫身上，产出“最佳跳跃候选”（CatJumpPlan）。
/// 扫描逻辑为纯数学，服务端与客户端均可运行（两端注册表一致，结果确定性相同）；
/// UI 提示只在本地可操作端（本地玩家/单机调试）且处于 Locomotion 状态时推送。
/// </summary>
[RequireComponent(typeof(CatCharacter))]
public class LandingSurfaceScanner : MonoBehaviour
{
    [Tooltip("候选搜索半径（米）")]
    [Range(0.1f, 3f)] public float m_Radius = 1.5f;
    [Tooltip("提示刷新间隔（秒），跳跃判定本身每次按键都会实时扫描")]
    [SerializeField] private float m_TipInterval = 0.15f;
    [Tooltip("目标点至少比脚高这么多才算“跳上”")]
    [SerializeField] private float m_MinHeight = 0.1f;
    [Tooltip("有移动输入时，候选方向与期望方向点积低于该值则过滤（背对不跳）")]
    [SerializeField] private float m_DirectionDot = 0.3f;

    private const int JumpTipId = 1;

    private CatCharacter m_Cat;
    private CatNetworkEntity m_Entity;
    private LandingSurface[] m_Results;
    private float m_NextTipTime;

    /// <summary>是否本地可操作端（本地玩家或单机调试），只有它才推送 UI 提示。</summary>
    private bool IsLocalView => m_Entity == null || m_Entity.CanReadLocalInput;

    private void Awake()
    {
        m_Results = new LandingSurface[16];
        m_Cat = GetComponent<CatCharacter>();
        m_Entity = GetComponent<CatNetworkEntity>();
    }

    private void Update()
    {
        // 非本地端（服务端上的他人猫、其他客户端的代理猫）不推提示
        if (!IsLocalView || Time.time < m_NextTipTime)
        {
            return;
        }
        m_NextTipTime = Time.time + m_TipInterval;

        // 只有地面移动状态才提示：跳跃中、坐下等状态不打扰
        bool inLocomotion = m_Cat.catFsm?.GetCurrentFsm() is CatFsmLocomotion;
        if (inLocomotion && TryGetBestCandidate(m_Cat.DesiredMoveDirection, out var plan))
        {
            TipsMgr.ShowTip(new TipsInfo
            {
                TipId = JumpTipId,
                TipsType = ENUM_TIPS_TYPE.TIPS_LANDING_SURFACE,
                Anchor = new TipAnchor { FixedPoint = plan.TargetPoint },
                Text = "跳上去",
                KeyHint = "空格",
            });
        }
        else
        {
            TipsMgr.HideTip(JumpTipId);
        }
    }

    /// <summary>
    /// 实时扫描并选出最佳跳跃候选。每次调用都重新查询（结果供 FSM 按键瞬间使用），
    /// 评分 = 方向匹配度为主、距离为辅；无候选返回 false。
    /// </summary>
    /// <param name="desiredDir">期望移动方向（世界空间水平面），用于过滤背对候选</param>
    public bool TryGetBestCandidate(Vector3 desiredDir, out CatJumpPlan plan)
    {
        plan = null;
        var origin = transform.position;
        var count = LandingSurfaceRegistry.QuerySphere(origin, m_Radius, m_Results);

        bool hasDir = desiredDir.sqrMagnitude > 0.01f;
        var dirNormalized = hasDir ? desiredDir.normalized : Vector3.zero;
        float bestScore = float.MinValue;

        for (int i = 0; i < count; i++)
        {
            var surface = m_Results[i];
            var point = surface.GetClosestStandPoint(origin);
            var toPoint = point - origin;

            // 只考虑“跳上”：目标明显高于当前位置
            if (toPoint.y < m_MinHeight)
            {
                continue;
            }

            float dirScore = 0f;
            if (hasDir)
            {
                var planar = new Vector3(toPoint.x, 0f, toPoint.z);
                if (planar.sqrMagnitude < 0.001f)
                {
                    continue; // 正头顶的候选暂不处理
                }
                dirScore = Vector3.Dot(dirNormalized, planar.normalized);
                if (dirScore < m_DirectionDot)
                {
                    continue; // 背对期望方向的候选不跳
                }
            }

            // 方向权重 2，距离权重 -0.1：优先顺方向，其次近
            float score = dirScore * 2f - toPoint.magnitude * 0.1f;
            if (score > bestScore)
            {
                bestScore = score;
                plan = new CatJumpPlan
                {
                    Surface = surface,
                    TargetPoint = point,
                };
            }
        }
        return plan != null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, m_Radius);
    }
}
