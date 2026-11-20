// 21.286: cây dừa Meshy + bệ gạch cũ (màu đỉnh) trên bản đồ mở.
// Thân/tán: texture Meshy, hai mặt (lá Meshy là mặt đơn). Bệ: màu đỉnh từ Blender (_UseVertexColor = 1), không UV.
Shader "PhoViet/OpenWorld/Palm"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _UseVertexColor ("Use mesh color", Range(0,1)) = 0
        _Smoothness ("Smoothness", Range(0,1)) = 0.08
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200
        Cull [_Cull]
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma multi_compile_instancing
        #pragma target 3.0
        sampler2D _MainTex; fixed4 _Color; half _UseVertexColor; half _Smoothness;
        struct Input { float2 uv_MainTex; float4 color : COLOR; fixed facing : VFACE; };
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 tex = _UseVertexColor > 0.5 ? fixed4(1,1,1,1) : tex2D(_MainTex, IN.uv_MainTex);
            fixed3 vc = _UseVertexColor > 0.5 ? IN.color.rgb : fixed3(1,1,1);
            o.Albedo = tex.rgb * vc * _Color.rgb;
            o.Normal = float3(0, 0, IN.facing < 0 ? -1 : 1);     // mặt sau của lá cũng nhận sáng đúng
            o.Metallic = 0; o.Smoothness = _Smoothness; o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
