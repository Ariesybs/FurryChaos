// 站立面 Gizmos 填充专用着色器（仅编辑器使用，Editor 目录不进入构建）。
// 与 Handles 实心绘制的区别：ZWrite On + ZTest LEqual，
// 填充面会被场景几何遮挡，多个透明填充面之间也能按深度正确互相遮挡。
Shader "Hidden/FurryChaos/SurfaceAreaFill"
{
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        ZTest LEqual
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;   // 配合 GL.Color 的顶点色
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
