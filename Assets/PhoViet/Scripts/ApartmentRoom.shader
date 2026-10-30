// Phòng căn hộ sau vách kính (25/10/2026, huongdan.md 21.51): ban ngày như atlas phố (Standard), về đêm tự phát sáng
// theo màu bảng phố của từng mặt (vách phòng ấm, máng đèn trần) với cường độ _PVNightLights mà DayNightLighting đặt
// mỗi khung (cùng mức bật đèn đường), nên phòng sáng dần lúc chạng vạng và tắt khi trời sáng.
Shader "PhoViet/Apartment Room"
{
    Properties
    {
        _MainTex ("Palette", 2D) = "white" {}
        _Glow ("Night glow", Float) = 1.15
        _Warm ("Warm tint", Color) = (1,0.86,0.66,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.2
        _StuccoDetail ("Plaster detail", 2D) = "gray" {}
        _StuccoScale ("Plaster repeats per metre", Float) = 1
        _StuccoStrength ("Plaster relief", Range(0,1)) = 0.20
        _StuccoContrast ("Paint variation", Range(0,0.3)) = 0.10
        _WallSmoothness ("Plaster smoothness", Range(0,1)) = 0.035
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:PVStuccoVertex
        #pragma target 3.0
        sampler2D _MainTex; half _Glow,_Glossiness; fixed4 _Warm; float _PVNightLights;
        struct Input { float2 uv_MainTex;float2 stuccoUV; };
        #include "WallStucco.cginc"
        void surf(Input IN,inout SurfaceOutputStandard o)
        {
            fixed3 c=tex2D(_MainTex,IN.uv_MainTex).rgb;
            o.Albedo=c;o.Normal=half3(0,0,1);o.Metallic=0;o.Smoothness=_Glossiness;o.Alpha=1;
            PVApplyStucco(IN.uv_MainTex,IN.stuccoUV,o);
            o.Emission=c*_Warm.rgb*_Glow*saturate(_PVNightLights);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
