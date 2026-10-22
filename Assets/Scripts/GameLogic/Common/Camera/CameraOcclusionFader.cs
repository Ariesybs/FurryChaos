using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 相机遮挡渐隐：相机与玩家之间的静态遮挡物变半透明，无遮挡后还原。
/// 挂在主相机上。遮挡物需使用 URP/Lit 或兼容 _Surface/_Color 属性的 Shader。
/// </summary>
public sealed class CameraOcclusionFader : MonoBehaviour
{
    [Header("跟随目标（玩家）")]
    [SerializeField] private Transform target;

    [Header("遮挡检测")]
    [SerializeField] private LayerMask occluderMask;       // 静态环境所在层
    [SerializeField, Min(0f)] private float sphereRadius = 0.3f; // 球体投射半径，防止擦边穿帮
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("渐变")]
    [SerializeField, Range(0.05f, 1f)] private float fadedAlpha = 0.3f;
    [SerializeField, Min(0.1f)] private float fadeSpeed = 8f;

    // URP/Lit 透明模式属性 ID（避免字符串查找开销）
    private static readonly int SurfaceId = Shader.PropertyToID("_Surface");
    private static readonly int BlendId = Shader.PropertyToID("_Blend");
    private static readonly int SrcBlendId = Shader.PropertyToID("_SrcBlend");
    private static readonly int DstBlendId = Shader.PropertyToID("_DstBlend");
    private static readonly int ZWriteId = Shader.PropertyToID("_ZWrite");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private const string TransparentKeyword = "_SURFACE_TYPE_TRANSPARENT";

    private sealed class FadeEntry
    {
        public Material[] originalShared;   // 原始共享材质，用于还原
        public Material[] fadedInstances;   // 透明化的材质实例
        public float currentAlpha = 1f;
        public bool occludedThisFrame;
    }

    private readonly Dictionary<Renderer, FadeEntry> entries = new();
    private readonly List<Renderer> restorePending = new();
    private RaycastHit[] hits = new RaycastHit[32];

    private void Awake()
    {
        
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 1. 重置帧标记
        foreach (var pair in entries)
        {
            pair.Value.occludedThisFrame = false;
        }

        // 2. 球体投射，收集本帧遮挡物
        Vector3 origin = transform.position;
        Vector3 toTarget = target.position - origin;
        float distance = toTarget.magnitude;
        if (distance > 0.001f)
        {
            int count = Physics.SphereCastNonAlloc(origin, sphereRadius, toTarget / distance, hits, distance, occluderMask, triggerInteraction);

            if (count == hits.Length) // 数组打满时扩容
            {
                hits = new RaycastHit[hits.Length * 2];
            }

            for (int i = 0; i < count; i++)
            {
                if (!hits[i].collider.TryGetComponent(out Renderer renderer))
                {
                    continue;
                }
                if (target.IsChildOf(renderer.transform))
                {
                    continue; // 排除玩家自身
                } 

                if (!entries.TryGetValue(renderer, out FadeEntry entry))
                {
                    entry = CreateFadeEntry(renderer);
                    if (entry == null)
                    {
                        continue;
                    }
                    entries.Add(renderer, entry);
                }
                entry.occludedThisFrame = true;
            }
        }

        // 3. 渐变 alpha，完全还原的移出字典
        restorePending.Clear();
        foreach (var pair in entries)
        {
            Renderer renderer = pair.Key;
            FadeEntry entry = pair.Value;

            float targetAlpha = entry.occludedThisFrame ? fadedAlpha : 1f;
            entry.currentAlpha = Mathf.MoveTowards(entry.currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
            SetAlpha(entry.fadedInstances, entry.currentAlpha);

            if (!entry.occludedThisFrame && entry.currentAlpha >= 1f)
            {
                renderer.sharedMaterials = entry.originalShared; // 还原原始材质
                foreach (Material mat in entry.fadedInstances)
                {
                    Destroy(mat);// 销毁实例，防止泄漏
                }
                restorePending.Add(renderer);
            }
        }
        foreach (Renderer renderer in restorePending)
        {
            entries.Remove(renderer);
        }
    }

    /// <summary>首次遮挡时：实例化材质并切换为 URP 透明模式。</summary>
    private FadeEntry CreateFadeEntry(Renderer renderer)
    {
        Material[] shared = renderer.sharedMaterials;
        var instances = new Material[shared.Length];
        for (int i = 0; i < shared.Length; i++)
        {
            if (shared[i] == null || !shared[i].HasProperty(SurfaceId))
            {
                return null; // 非 URP/Lit 材质，跳过
            } 
            instances[i] = new Material(shared[i]);

            Material mat = instances[i];
            mat.SetFloat(SurfaceId, 1f); // Transparent
            mat.SetFloat(BlendId, 0f);   // Alpha 混合
            mat.SetFloat(SrcBlendId, (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat(DstBlendId, (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat(ZWriteId, 0f);
            mat.EnableKeyword(TransparentKeyword);
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        renderer.sharedMaterials = instances;
        return new FadeEntry { originalShared = shared, fadedInstances = instances };
    }

    private static void SetAlpha(Material[] materials, float alpha)
    {
        foreach (Material mat in materials)
        {
            Color color = mat.GetColor(BaseColorId);
            color.a = alpha;
            mat.SetColor(BaseColorId, color);
        }
    }

    private void OnDisable()
    {
        // 组件被禁用时立即还原所有材质，避免残留半透明
        foreach (var pair in entries)
        {
            if (pair.Key == null) continue;
            pair.Key.sharedMaterials = pair.Value.originalShared;
            foreach (Material mat in pair.Value.fadedInstances)
            {
                Destroy(mat);
            }
        }
        entries.Clear();
    }
}