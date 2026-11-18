using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 运行时调试可视化：在 Game 窗口和 Scene 窗口绘制点、线、射线、圆。
/// 单文件、零依赖（仅用 UnityEngine），拷贝到任意工程即可使用。
/// 发布版本中所有 Draw 调用会被编译期剔除（Conditional），调用方无需 #if。
/// duration ≤ 0 表示只存活一帧；颜色中的 alpha 生效；depthTest 控制是否被场景遮挡。
/// </summary>
public static class ViewDebug
{
    // ============================ 公开 API ============================

    /// <summary>画一个面向相机的点（面片）。</summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void DrawPoint(Vector3 position, float size = 0.05f, Color? color = null, float duration = 0f, bool depthTest = false)
    {
        Enqueue(new DrawCommand
        {
            Kind = DrawKind.Point, A = position, Radius = size,
            Color = color ?? Color.white, DepthTest = depthTest
        }, duration);
    }

    /// <summary>画一条线段。</summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void DrawLine(Vector3 start, Vector3 end, Color? color = null, float duration = 0f, bool depthTest = false)
    {
        Enqueue(new DrawCommand
        {
            Kind = DrawKind.Line, A = start, B = end,
            Color = color ?? Color.white, DepthTest = depthTest
        }, duration);
    }

    /// <summary>画一条射线（线段 + 末端箭头）。direction 会归一化。</summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void DrawRay(Vector3 origin, Vector3 direction, float length = 1f, Color? color = null, float duration = 0f, bool depthTest = false)
    {
        if (direction.sqrMagnitude < 1e-8f) return;
        Vector3 dir = direction.normalized;
        Vector3 end = origin + dir * length;
        var c = color ?? Color.white;

        Enqueue(new DrawCommand { Kind = DrawKind.Line, A = origin, B = end, Color = c, DepthTest = depthTest }, duration);

        // 箭头：取垂直于方向的任意轴，画两根短线
        Vector3 side = Vector3.Cross(dir, Mathf.Abs(dir.y) < 0.99f ? Vector3.up : Vector3.right).normalized;
        float head = length * 0.15f;
        Enqueue(new DrawCommand { Kind = DrawKind.Line, A = end, B = end - dir * head + side * head * 0.5f, Color = c, DepthTest = depthTest }, duration);
        Enqueue(new DrawCommand { Kind = DrawKind.Line, A = end, B = end - dir * head - side * head * 0.5f, Color = c, DepthTest = depthTest }, duration);
    }

    /// <summary>画一个圆。normal 为圆所在平面的法线，默认水平面。</summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void DrawCircle(Vector3 center, float radius, Vector3? normal = null, Color? color = null, float duration = 0f, int segments = 32, bool depthTest = false)
    {
        Enqueue(new DrawCommand
        {
            Kind = DrawKind.Circle, A = center, B = normal ?? Vector3.up, Radius = radius,
            Segments = segments, Color = color ?? Color.white, DepthTest = depthTest
        }, duration);
    }

    /// <summary>清除所有存活中的调试图元。</summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Clear()
    {
        s_Commands.Clear();
    }

    // ============================ 数据结构 ============================

    private enum DrawKind { Point, Line, Circle }

    private struct DrawCommand
    {
        public DrawKind Kind;
        public Vector3 A;     // 点:中心  线:起点  圆:圆心
        public Vector3 B;     // 线:终点  圆:法线
        public float Radius;  // 点:尺寸  圆:半径
        public int Segments;  // 圆:分段数
        public Color Color;
        public float ExpireAt; // < 0 表示仅存活入队时的那一帧
        public int Frame;      // 入队帧号，配合 ExpireAt < 0 使用
        public bool DepthTest;
    }

    private static readonly List<DrawCommand> s_Commands = new(128);
    private static Material s_DepthMat;
    private static Material s_NoDepthMat;
    private static bool s_Subscribed;

    // ============================ 初始化 ============================

    static ViewDebug() => EnsureSubscribed();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnSubsystemRegistration()
    {
        // 只清命令：域重载关闭时旧订阅和材质仍然有效，重复订阅会导致重复绘制
        s_Commands.Clear();
    }
#endif

    /// <summary>按当前渲染管线订阅相机渲染完成回调。可重复调用，内部防重。</summary>
    private static void EnsureSubscribed()
    {
        if (s_Subscribed) return;
        s_Subscribed = true;

        if (GraphicsSettings.currentRenderPipeline != null)
        {
            // URP/HDRP：Game 视图与 Scene 视图相机都会触发
            RenderPipelineManager.endCameraRendering += OnEndCameraRenderingSrp;
        }
        else
        {
            // 内置管线：仅 Game 视图触发（Scene 视图如需支持可另加 SceneView.duringSceneGui）
            Camera.onPostRender += OnPostRenderBiRP;
        }
    }

    // ============================ 入队 ============================

    private static void Enqueue(in DrawCommand cmd, float duration)
    {
        EnsureSubscribed();

        var c = cmd;
        c.Frame = Time.frameCount;
        // duration ≤ 0 → ExpireAt 置 -1，表示只画入队时的这一帧（暂停时不消失）
        // duration > 0 → 按未缩放时间过期，编辑器暂停时计时不流失
        c.ExpireAt = duration <= 0f ? -1f : Time.unscaledTime + duration;
        s_Commands.Add(c);
    }

    // ============================ 渲染 ============================

    private static void OnEndCameraRenderingSrp(ScriptableRenderContext ctx, Camera cam)
    {
        // 只画正式相机，跳过资产预览缩略图、反射探针等
        if (cam.cameraType != CameraType.Game && cam.cameraType != CameraType.SceneView)
            return;
        Render(cam);
    }

    private static void OnPostRenderBiRP(Camera cam)
    {
        if (cam.cameraType != CameraType.Game) return;
        Render(cam);
    }

    private static void Render(Camera cam)
    {
        PurgeExpired();
        if (s_Commands.Count == 0) return;

        EnsureMaterials();
        RenderPass(cam, s_DepthMat, depthTest: true);
        RenderPass(cam, s_NoDepthMat, depthTest: false);
    }

    private static void EnsureMaterials()
    {
        if (s_DepthMat != null) return;
        // 内置顶点色 Shader：支持顶点色透明混合，且 _ZTest 可配置
        var shader = Shader.Find("Hidden/Internal-Colored");
        s_DepthMat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        s_DepthMat.SetInt("_ZTest", (int)CompareFunction.LessEqual);
        s_NoDepthMat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        s_NoDepthMat.SetInt("_ZTest", (int)CompareFunction.Always);
    }

    /// <summary>
    /// 惰性清理：每帧必然渲染，过期图元在渲染前移除即可，不需要独立的 Update 心跳。
    /// 一帧内多个相机重复清理无害（首相机清完，后续相机读到的是已清理列表）。
    /// </summary>
    private static void PurgeExpired()
    {
        float now = Time.unscaledTime;
        for (int i = s_Commands.Count - 1; i >= 0; i--)
        {
            var cmd = s_Commands[i];
            bool frameExpired = cmd.ExpireAt < 0f && Time.frameCount > cmd.Frame;
            bool timeExpired = cmd.ExpireAt >= 0f && now >= cmd.ExpireAt;
            if (frameExpired || timeExpired)
                s_Commands.RemoveAt(i);
        }
    }

    private static void RenderPass(Camera cam, Material mat, bool depthTest)
    {
        mat.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.identity); // 命令存的是世界坐标，保持相机视图矩阵即可

        GL.Begin(GL.LINES);
        foreach (var cmd in s_Commands)
        {
            if (cmd.DepthTest != depthTest || cmd.Kind == DrawKind.Point) continue;
            GL.Color(cmd.Color);
            if (cmd.Kind == DrawKind.Line)
            {
                GL.Vertex(cmd.A);
                GL.Vertex(cmd.B);
            }
            else
            {
                EmitCircle(cmd);
            }
        }
        GL.End();

        // 点画成面向当前相机的面片，保证有可见尺寸
        GL.Begin(GL.QUADS);
        foreach (var cmd in s_Commands)
        {
            if (cmd.DepthTest != depthTest || cmd.Kind != DrawKind.Point) continue;
            GL.Color(cmd.Color);
            Vector3 right = cam.transform.right * cmd.Radius;
            Vector3 up = cam.transform.up * cmd.Radius;
            GL.Vertex(cmd.A - right - up);
            GL.Vertex(cmd.A - right + up);
            GL.Vertex(cmd.A + right + up);
            GL.Vertex(cmd.A + right - up);
        }
        GL.End();

        GL.PopMatrix();
    }

    private static void EmitCircle(in DrawCommand cmd)
    {
        Vector3 normal = cmd.B.sqrMagnitude < 1e-8f ? Vector3.up : cmd.B.normalized;
        // 任取一条不平行轴，构造圆所在平面的正交基
        Vector3 tangent = Vector3.Cross(normal, Mathf.Abs(normal.y) < 0.99f ? Vector3.up : Vector3.right).normalized;
        Vector3 bitangent = Vector3.Cross(normal, tangent);
        int seg = Mathf.Max(8, cmd.Segments);

        Vector3 prev = cmd.A + tangent * cmd.Radius;
        for (int i = 1; i <= seg; i++)
        {
            float angle = i * Mathf.PI * 2f / seg;
            Vector3 next = cmd.A + (tangent * Mathf.Cos(angle) + bitangent * Mathf.Sin(angle)) * cmd.Radius;
            GL.Vertex(prev);
            GL.Vertex(next);
            prev = next;
        }
    }
}