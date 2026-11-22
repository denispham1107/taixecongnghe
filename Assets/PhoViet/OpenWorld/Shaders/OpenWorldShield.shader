// Bản đồ mở Phố Việt (21.312): khiên năng lượng của xe người chơi — cộng sáng, dùng lưới Blender PV_Khien.fbx
// (ArtSource/Blender/OpenWorld/build_khien.py).
// _Mode 0 = vỏ elip trơn (viền fresnel sáng + mờ ở giữa); 1 = khung lục giác (UV0.x 0 mép ô → 1 trong; UV1 = (ngẫu nhiên ô, độ cao));
//       2 = mặt ô lục giác (ô lóe sáng ngẫu nhiên); 3 = vành sáng dưới đất (UV0.y ngang dải, chạy vòng theo UV0.x).
// Chung: dải quét chạy từ đáy lên đỉnh, gợn sóng chỗ bị trúng (_Hit: xyz hướng trong hệ vật thể, w = giây từ lúc trúng),
//        _Alpha mờ dần khi bật/tắt.
Shader "PhoViet/OpenWorld/Shield"
{
    Properties
    {
        [HDR] _Color ("Màu năng lượng", Color) = (0.25, 1.6, 1.35, 1)
        _Mode ("0 vỏ, 1 khung, 2 ô, 3 vành", Float) = 0
        _Alpha ("Độ hiện", Range(0, 1)) = 1
        _Hit ("Hướng trúng (xyz) + giây (w)", Vector) = (0, 0, 1, 99)
        _Center ("Tâm khiên (hệ vật thể)", Vector) = (0, 0.55, 0, 0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent+2" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend One One
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            half4 _Color; float _Mode, _Alpha; float4 _Hit, _Center;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; float2 uv1 : TEXCOORD1; };
            struct v2f
            {
                float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float2 uv1 : TEXCOORD1;
                float3 wn : TEXCOORD2; float3 vd : TEXCOORD3; float3 op : TEXCOORD4;
            };
            v2f vert (appdata v)
            {
                v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.uv1 = v.uv1; o.op = v.vertex.xyz;
                float3 wp = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.wn = UnityObjectToWorldNormal(v.normal);
                o.vd = _WorldSpaceCameraPos - wp;
                return o;
            }
            half4 frag (v2f i) : SV_Target
            {
                float t = _Time.y;
                float3 n = normalize(i.wn), vdir = normalize(i.vd);
                float fres = pow(1 - saturate(abs(dot(n, vdir))), 3.0);         // 21.314: viền mỏng hơn (2,4 → 3,0)
                float h = (_Mode > 0.5 && _Mode < 2.5) ? i.uv1.y : saturate((i.op.y - _Center.y + 1.3) / 2.6);
                float band = exp(-pow((h - frac(t * 0.75)) * 7.0, 2));
                float3 dir = normalize(i.op - _Center.xyz);
                float hd = length(dir - normalize(_Hit.xyz));
                float ring = exp(-pow((hd - _Hit.w * 3.2) * 5.0, 2)) * saturate(1 - _Hit.w * 1.6) + saturate(1 - hd * 1.6) * saturate(1 - _Hit.w * 4.0);
                float k;
                if (_Mode < 0.5)
                {
                    // 21.314 (người dùng: bên trong còn đục): giữa khiên gần như trong suốt, sáng dồn ra viền fresnel
                    k = 0.012 + fres * 0.85 + band * 0.05 + ring * 0.9;
                }
                else if (_Mode < 1.5)
                {
                    float rnd = i.uv1.x;
                    float edgeL = pow(saturate(1 - i.uv.x), 2.0) + 0.06;          // 21.314: vạch mảnh hơn (1,4 → 2,0; nền 0,18 → 0,06)
                    float flick = 0.72 + 0.28 * sin(t * 7.0 + rnd * 40.0);
                    k = edgeL * (0.12 + fres * 1.1 + band * 0.7 + ring * 2.2) * flick * lerp(0.55, 1.0, fres);
                }
                else if (_Mode < 2.5)
                {
                    float rnd = i.uv1.x;
                    float flash = smoothstep(0.9, 1.0, frac(rnd * 13.7 + t * 0.55));
                    k = (0.008 + flash * 0.28 + band * 0.05 + ring * 0.7 + fres * 0.06) * (1 - i.uv.x * 0.45);
                }
                else
                {
                    float across = 1 - abs(i.uv.y * 2 - 1);
                    k = pow(saturate(across), 1.8) * (0.55 + 0.45 * sin(i.uv.x * 6.2832 * 12 - t * 9.0));
                }
                return half4(_Color.rgb * k * _Alpha, 1);
            }
            ENDCG
        }
    }
}
