// 21.271 — vệt sóng sau tàu trên sông bản đồ mở. Dải băng theo đường tàu vừa chạy (OpenWorldBoats dựng lại mỗi khung):
//   uv.x = tuổi 0 (mũi tàu) → 1 (cuối vệt), uv.y = −1 … 1 ngang vệt, color.a = độ mạnh (theo tốc độ tàu).
// Hai cánh sóng chữ V ở mép + dòng bọt xoáy giữa, nhạt dần theo tuổi; đỉnh dời theo cùng hàm sóng với mặt sông.
Shader "PhoViet/OpenWorld/BoatWake"
{
    Properties
    {
        _Foam ("Bọt (xám)", 2D) = "black" {}
        _FoamColor ("Màu bọt", Color) = (0.93, 0.96, 0.95, 1)
        _Lift ("Nâng khỏi mặt nước (m)", Float) = 0.06
    }
    SubShader
    {
        Tags { "Queue"="Transparent-10" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            Offset -1, -1
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "OpenWorldRiverWaves.cginc"
            sampler2D _Foam; fixed4 _FoamColor; float _Lift;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float2 wxz : TEXCOORD1; fixed a : TEXCOORD2; UNITY_FOG_COORDS(3) };
            v2f vert(appdata v)
            {
                v2f o; float2 s;
                float3 w = mul(unity_ObjectToWorld, v.vertex).xyz;
                w.y += OWWaveHeight(w.xz, _Time.y, s) + _Lift;
                o.pos = UnityWorldToClipPos(float4(w, 1));
                o.uv = v.uv; o.wxz = w.xz; o.a = v.color.a;
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float age = i.uv.x, x = abs(i.uv.y);
                float tex = tex2D(_Foam, i.wxz / 4.2 + float2(_Time.y * 0.03, 0)).r;
                float tex2 = tex2D(_Foam, i.wxz / 1.7 - float2(0, _Time.y * 0.05)).r;
                float arm = smoothstep(0.55, 0.86, x) * (1 - smoothstep(0.9, 1.0, x));          // hai cánh chữ V ở mép
                float core = (1 - smoothstep(0.0, 0.5, x)) * (1 - smoothstep(0.0, 0.8, age));    // bọt xoáy giữa (chân vịt)
                float wash = (1 - smoothstep(0.0, 0.95, x)) * 0.28;                              // nước xáo nhạt giữa hai cánh
                float fade = pow(saturate(1 - age), 1.2) * smoothstep(0.0, 0.03, age);
                float f = (arm * (0.85 + tex * 1.6) + core * (1.1 + tex2 * 1.6) + wash * (0.4 + tex)) * fade * i.a;
                fixed3 c = _FoamColor.rgb * (ShadeSH9(float4(0, 1, 0, 1)) * 0.7 + _LightColor0.rgb * 0.45);
                fixed4 col = fixed4(c, saturate(f));
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    Fallback Off
}
