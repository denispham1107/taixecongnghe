Shader "PhoViet/Street Wall Finish"
{
    Properties
    {
        _MainTex ("Palette", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Original smoothness", Range(0,1)) = 0.12
        _Metallic ("Original metallic", Range(0,1)) = 0
        _StuccoDetail ("Plaster detail: height / Y / unused / X", 2D) = "gray" {}
        _StuccoScale ("Plaster repeats per metre", Float) = 0.65
        _StuccoStrength ("Plaster relief", Range(0,1)) = 0.42
        _StuccoContrast ("Paint variation", Range(0,0.3)) = 0.28
        _WallSmoothness ("Plaster smoothness", Range(0,1)) = 0.015
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:PVStuccoVertex
        #pragma target 3.0
        sampler2D _MainTex;fixed4 _Color;half _Glossiness,_Metallic;
        struct Input { float2 uv_MainTex;float2 stuccoUV; };
        #include "WallStucco.cginc"
        void surf(Input IN,inout SurfaceOutputStandard o)
        {
            o.Albedo=tex2D(_MainTex,IN.uv_MainTex).rgb*_Color.rgb;
            o.Normal=half3(0,0,1);o.Metallic=_Metallic;o.Smoothness=_Glossiness;o.Alpha=1;
            PVApplyStucco(IN.uv_MainTex,IN.stuccoUV,o);
        }
        ENDCG
    }
    FallBack "Standard"
}
