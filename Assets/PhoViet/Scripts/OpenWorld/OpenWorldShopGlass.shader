// 21.272 — kính mặt tiền tầng trệt nhà phố: trong suốt khi nhìn thẳng, phản chiếu trời/phố (Reflection Probe hoặc trời
// môi trường) mạnh dần khi nhìn xiên (Fresnel), ánh nắng lóa, ám xanh nhẹ. Vẽ sau nội thất (Transparent), không ghi độ sâu.
Shader "PhoViet/OpenWorld/ShopGlass"
{
    Properties
    {
        _Tint ("Ám màu kính", Color) = (0.80, 0.90, 0.93, 1)
        _MinAlpha ("Độ đục khi nhìn thẳng", Range(0, 1)) = 0.10
        _MaxAlpha ("Độ đục khi nhìn xiên", Range(0, 1)) = 0.72
        _Reflect ("Độ phản chiếu", Range(0, 2)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            fixed4 _Tint; float _MinAlpha, _MaxAlpha, _Reflect;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos : SV_POSITION; float3 w : TEXCOORD0; float3 n : TEXCOORD1; UNITY_FOG_COORDS(2) };
            v2f vert(appdata v)
            {
                v2f o; UNITY_SETUP_INSTANCE_ID(v);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.w = mul(unity_ObjectToWorld, v.vertex).xyz; o.n = UnityObjectToWorldNormal(v.normal);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float3 v = normalize(_WorldSpaceCameraPos - i.w);
                float3 n = normalize(i.n); if (dot(n, v) < 0) n = -n;
                float ndv = saturate(dot(n, v));
                float fres = 0.04 + 0.96 * pow(1 - ndv, 4);
                float3 r = reflect(-v, n);
                float3 env = DecodeHDR(UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, r, 1.5), unity_SpecCube0_HDR);
                float3 l = normalize(_WorldSpaceLightPos0.xyz);
                float spec = pow(saturate(dot(r, l)), 300) * 3;
                float3 c = lerp(_Tint.rgb * 0.35, env * _Reflect, saturate(fres * 1.4)) + _LightColor0.rgb * spec;
                float a = lerp(_MinAlpha, _MaxAlpha, saturate(fres * 1.3)) + spec * 0.5;
                fixed4 col = fixed4(c, saturate(a));
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    Fallback Off
}
