// Bản đồ mở Phố Việt (21.294): hộp nhà phố ở xa (LOD1_Hop) mang ẢNH MẶT TIỀN THẬT chụp từ lưới LOD0 (menu 18,
// PhoVietOpenWorldFarFacadeBuilder). Một atlas chung cho 93 mẫu nhà, UV theo từng mặt hộp (trước/sau/2 hông/mái).
// Ánh sáng Standard như shader Triplanar của nhà thật → chuyển LOD không đổi màu.
Shader "PhoViet/OpenWorld/FarFacade"
{
    Properties
    {
        _Color ("Màu", Color) = (1,1,1,1)
        _MainTex ("Atlas mặt tiền", 2D) = "white" {}
        _Glossiness ("Độ bóng", Range(0,1)) = 0.12
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #pragma multi_compile_instancing
        sampler2D _MainTex;
        fixed4 _Color;
        half _Glossiness;
        struct Input { float2 uv_MainTex; };
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color.rgb;
            o.Metallic = 0;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
