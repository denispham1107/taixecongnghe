// Bản đồ mở Phố Việt (21.365e): nước văng 3D khi xe rơi sông/biển — lưới dựng trong Blender (ow_water_splash.py export_3d),
// thay ảnh phẳng 256² bị bể/mờ khi camera lia gần. Giống vật liệu đã duyệt trong Blender (clear_water_mat):
//   thân gần trong suốt (_Base) + mép sáng theo góc nhìn (Fresnel × _Edge) + chấm lóa nắng (N·H)^140 trên mặt có gợn nhiễu
//   + phản chiếu trời tự tính (như trời Blender) + bọt trắng ở mép trên (_RimFoam, theo chiều cao _CrownH).
// _Foam 1 = ụ bọt chân (trắng mờ). _Fade: tắt dần cuối hiệu ứng. Hai mặt (Cull Off), không ghi độ sâu.
Shader "PhoViet/OpenWorld/WaterSplash"
{
    Properties
    {
        _Tint ("Màu nước", Color) = (0.42, 0.58, 0.55, 1)
        _Base ("Độ đục nền", Range(0, 1)) = 0.06
        _Edge ("Mép sáng (Fresnel)", Range(0, 2)) = 0.62
        _Glint ("Lóa nắng", Range(0, 3)) = 1
        _RimFoam ("Bọt mép trên", Range(0, 1)) = 0.85
        _Foam ("1 = ụ bọt, 0 = nước", Float) = 0
        _Fade ("Hiện (1) → tắt (0)", Range(0, 1)) = 1
        _CrownH ("Chiều cao khung hiện tại (m, thế giới)", Float) = 3.2
        _Aerate ("Vệt bọt sục khí", Range(0, 1)) = 0.55
        _Wave ("Tấm nước uốn lượn (m)", Range(0, 0.5)) = 0.16
        _Flow ("Tốc độ sóng chạy lên", Float) = 2.6
    }
    SubShader
    {
        Tags { "Queue"="Transparent+5" "RenderType"="Transparent" "IgnoreProjector"="True" }
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
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            half4 _Tint; half _Base, _Edge, _Glint, _RimFoam, _Foam, _Fade, _CrownH, _Aerate, _Wave, _Flow;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float3 wp : TEXCOORD0; float3 wn : TEXCOORD1; float h : TEXCOORD2; UNITY_FOG_COORDS(3) float edge : TEXCOORD4; };
            float hash3(float3 p) { p = frac(p * 0.3183099 + 0.1); p *= 17.0; return frac(p.x * p.y * p.z * (p.x + p.y + p.z)); }
            float vnoise3(float3 x)
            {
                float3 i = floor(x), f = frac(x); f = f * f * (3 - 2 * f);
                return lerp(lerp(lerp(hash3(i), hash3(i + float3(1, 0, 0)), f.x), lerp(hash3(i + float3(0, 1, 0)), hash3(i + float3(1, 1, 0)), f.x), f.y),
                            lerp(lerp(hash3(i + float3(0, 0, 1)), hash3(i + float3(1, 0, 1)), f.x), lerp(hash3(i + float3(0, 1, 1)), hash3(i + float3(1, 1, 1)), f.x), f.y), f.z);
            }
            v2f vert (appdata v)
            {
                v2f o;
                o.wp = mul(unity_ObjectToWorld, v.vertex).xyz; o.wn = UnityObjectToWorldNormal(v.normal);
                float originY = unity_ObjectToWorld._m13;                          // gốc lưới = mặt nước
                o.h = saturate((o.wp.y - originY) / max(_CrownH, 0.05));
                o.edge = v.color.r;                                                  // khoảng cách tới mép rách (Blender export_3d): 0 mép → 1 ≥ 0,6 m
                // 21.365g (người dùng: «vẫn còn giống thủy tinh nhô lên, thêm hiệu ứng sóng nước khi nước bị bắn lên»): tấm nước
                // uốn lượn liên tục — sóng chạy NGƯỢC LÊN theo dòng nước bắn (nhiễu trôi theo −y), phần cao rung mạnh hơn; che cả
                // độ giật của 20 khung lưới.
                float t = _Time.y;
                float wv = (vnoise3(o.wp * 1.3 + float3(0, -t * _Flow, 0)) - 0.5) + 0.5 * (vnoise3(o.wp * 3.1 + float3(t * 1.3, -t * _Flow * 1.4, 0)) - 0.5);
                o.wp += o.wn * wv * _Wave * (0.3 + o.h) * (1 - _Foam * 0.5);
                o.pos = UnityWorldToClipPos(o.wp);
                UNITY_TRANSFER_FOG(o, o.pos); return o;
            }
            half4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {
                float3 n = normalize(i.wn) * (facing > 0 ? 1 : -1);
                float3 v = normalize(_WorldSpaceCameraPos - i.wp);
                // gợn nhỏ trên mặt nước → lóa vỡ thành chấm lấp lánh (như Bump nhiễu trong Blender)
                // 21.365g: gợn chạy lên theo dòng nước (2 lớp tốc độ khác nhau) → lóa sáng trôi liên tục như mặt nước thật
                float t = _Time.y;
                float3 q = i.wp * 2.6 + float3(0, -t * _Flow, 0), q2 = i.wp * 6.3 + float3(t * .7, -t * _Flow * 1.7, 0);
                float3 g = float3(vnoise3(q), vnoise3(q + 17.1), vnoise3(q + 31.7)) - 0.5;
                float3 g2 = float3(vnoise3(q2), vnoise3(q2 + 5.3), vnoise3(q2 + 11.9)) - 0.5;
                float3 pn = normalize(n + g * 0.55 + g2 * 0.35);
                float fres = pow(1 - saturate(dot(n, v)), 1.5);
                float3 L = normalize(_WorldSpaceLightPos0.xyz); float3 H = normalize(L + v);
                float glint = pow(saturate(dot(pn, H)), 140) * _Glint;
                // phản chiếu: trời tự tính như trời trong Blender đã duyệt (dưới chân trời tối như mặt sông, chân trời sáng, trên xanh)
                // — cảnh không có ảnh phản chiếu nướng sẵn (unity_SpecCube0 ra màu rác: nước văng thành xanh ngọc đặc khi thử).
                float ry = reflect(-v, pn).y;
                float3 refl = ry < 0 ? lerp(float3(0.80, 0.86, 0.90), float3(0.05, 0.09, 0.09), saturate(-ry * 4))
                                     : lerp(float3(0.80, 0.86, 0.90), float3(0.33, 0.52, 0.80), saturate(ry * 2));
                refl *= 0.6 + 0.4 * _LightColor0.rgb;
                float breakup = smoothstep(0.42, 0.62, vnoise3(i.wp * 5.5));
                float foamTop = smoothstep(0.72, 1.0, i.h) * _RimFoam * breakup;
                // 21.365f: nước văng thật lẫn khí → vệt bọt trắng chạy dọc tấm nước (nhiễu kéo dài theo chiều đứng), dày hơn ở
                // nửa trên và ở mép nghiêng (Fresnel) — bỏ cảm giác «ly thủy tinh bể»
                float streak = vnoise3(float3(i.wp.x * 4.5, i.wp.y * 0.9 - t * _Flow * .5, i.wp.z * 4.5)) * vnoise3(float3(i.wp.x * 9.0, i.wp.y * 1.6 - t * _Flow * .8, i.wp.z * 9.0) + 7.3);   // vệt bọt chảy theo dòng nước
                float aer = smoothstep(0.18, 0.42, streak) * _Aerate * (0.35 + 0.65 * i.h) * (0.6 + 0.4 * fres);
                foamTop = saturate(foamTop + aer);
                float3 col; float a;
                if (_Foam > 0.5)
                {   // ụ bọt chân: trắng mờ, vỡ vụn
                    col = float3(0.93, 0.96, 0.97) * (0.75 + 0.25 * _LightColor0.rgb);
                    a = (0.35 + fres * 0.2) * (0.55 + 0.45 * breakup);
                }
                else
                {
                    col = lerp(_Tint.rgb * (0.55 + 0.45 * _LightColor0.rgb), refl, saturate(0.15 + fres * 0.85));
                    col = lerp(col, float3(0.95, 0.97, 0.98), foamTop);
                    col = lerp(col, float3(0.92, 0.95, 0.96), fres * fres * 0.55);    // mép nước trắng như bọt, không viền xanh sắc như mép kính
                    col += glint * _LightColor0.rgb;
                    a = saturate(_Base + fres * _Edge + glint + foamTop * 0.75);
                }
                // 21.365g: mép rách TAN DẦN + lởm chởm, gợn trôi theo dòng nước (nước thật mỏng dần ra mép rồi vỡ thành tua/giọt)
                // thay vì đường cắt sắc như mảnh kính vỡ; vùng sắp tan ánh trắng như bụi nước.
                float en = vnoise3(i.wp * 8.0 + float3(0, -t * _Flow, 0)) * 0.65 + vnoise3(i.wp * 19.0 + float3(0, -t * _Flow * 1.5, 0)) * 0.35;
                float edgeA = saturate((i.edge * 1.8 - 0.25 + (en - 0.5) * 1.1) * 2.6);
                col = lerp(float3(0.93, 0.96, 0.97), col, saturate(i.edge * 1.6));
                a *= edgeA;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return half4(col, a * _Fade);
            }
            ENDCG
        }
    }
}
