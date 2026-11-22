// 21.271 — mặt sông bản đồ mở (thay «River Water Mirror» của phố cuộn: shader đó cần camera gương + texture nhiễu
// của game cũ nên ở bản đồ mở chỉ ra một màu xanh thẫm).
// Lưới mặt sông dựng bằng Blender (ArtSource/Blender/OpenWorld/build_river_surface.py):
//   uv0   = (s, d) mét theo dòng chảy: s dọc tim sông, d ngang (dương = bên trái chiều chảy)
//   color = r: gần kè (1 sát kè → 0 cách ≥ 10 m), g/b: hướng dòng chảy (x, z) * 0.5 + 0.5, a: 1
// Gồm: sóng dài dời đỉnh (OpenWorldRiverWaves.cginc, tàu nhấp nhô theo cùng hàm), 3 lớp gợn texture pháp tuyến
// (2 lớp trôi theo dòng + 1 lớp gió), phản chiếu từ Reflection Probe nướng sẵn (box projection, trộn 2 probe),
// Fresnel, màu nước sông Sài Gòn (xanh rêu thẫm, sát kè ngả bùn), ánh nắng + lấp lánh, bọt vỗ kè, nhận bóng đổ, sương.
Shader "PhoViet/OpenWorld/River"
{
    Properties
    {
        _WaveNormal ("Gợn sóng (normal)", 2D) = "bump" {}
        _Foam ("Bọt (xám)", 2D) = "black" {}
        _DeepColor ("Nước sâu", Color) = (0.035, 0.105, 0.105, 1)
        _MidColor ("Nước loang", Color) = (0.07, 0.16, 0.14, 1)
        _ShoreColor ("Sát kè (bùn)", Color) = (0.19, 0.2, 0.13, 1)
        _FoamColor ("Bọt", Color) = (0.92, 0.95, 0.93, 1)
        _ReflectTint ("Màu phản chiếu", Color) = (0.86, 0.92, 0.96, 1)
        _Tiling ("Ô gợn: dòng 1, dòng 2, gió (m)", Vector) = (14, 5.5, 3.2, 0)
        _FlowSpeed ("Tốc độ dòng (m/s)", Float) = 0.55
        _NormalStrength ("Độ gợn", Range(0, 2)) = 0.85
        _MinReflect ("Phản chiếu tối thiểu", Range(0, 1)) = 0.18
        _MaxReflect ("Phản chiếu tối đa (góc là)", Range(0, 1)) = 0.92
        _ReflectBlur ("Độ nhòe phản chiếu (mip)", Range(0, 6)) = 1.2
        _SunSpec ("Ánh nắng", Float) = 2.2
        _Sparkle ("Lấp lánh", Float) = 3.5
        _ShoreFoam ("Bọt vỗ kè", Range(0, 2)) = 1
        _ShadowStrength ("Độ đậm bóng", Range(0, 1)) = 0.55
        // 21.302/21.302c: mặt biển bao quanh bán đảo dùng chung shader, GIỐNG HỆT mặt sông (màu, gợn, bọt, nắng, phản chiếu):
        // 1 = biển → dùng 4 probe phản chiếu của sông nhưng trộn theo khoảng cách tới probe (biển không có toạ độ dọc tim sông),
        // điểm phản chiếu kẹp vào hộp probe rồi chuyển dần sang hướng phản xạ thẳng khi xa hộp, độ dốc sóng tính theo điểm ảnh
        // (lưới biển ở xa thưa hơn lưới sông).
        _SeaMode ("Biển (1 = mặt biển)", Float) = 0
        // 21.302c: không còn dùng (biển lấy đúng _DeepColor/_MidColor của sông); giữ để vật liệu cũ không lỗi
        _SeaDeep ("(không dùng) Biển: nước sâu", Color) = (0.03, 0.1, 0.13, 1)
        _SeaMid ("(không dùng) Biển: nước loang", Color) = (0.06, 0.155, 0.17, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+10" }
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma multi_compile_fog
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "UnityStandardUtils.cginc"
            #include "OpenWorldRiverWaves.cginc"

            sampler2D _WaveNormal, _Foam;
            fixed4 _DeepColor, _MidColor, _ShoreColor, _FoamColor, _ReflectTint, _SeaDeep, _SeaMid;
            float4 _Tiling;
            float _FlowSpeed, _NormalStrength, _MinReflect, _MaxReflect, _ReflectBlur, _SunSpec, _Sparkle, _ShoreFoam, _ShadowStrength, _SeaMode;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 world : TEXCOORD0;
                float2 flowUV : TEXCOORD1;
                fixed4 color : TEXCOORD2;
                float2 slope : TEXCOORD3;
                SHADOW_COORDS(4)
                UNITY_FOG_COORDS(5)
            };

            v2f vert(appdata v)
            {
                v2f o;
                float3 w = mul(unity_ObjectToWorld, v.vertex).xyz;
                float2 slope;
                // sát kè sóng nhỏ lại để mép nước không hở khỏi chân kè
                float h = OWWaveHeight(w.xz, _Time.y, slope) * (1 - v.color.r * 0.7);
                w.y += h;
                o.pos = UnityWorldToClipPos(float4(w, 1));
                o.world = w; o.flowUV = v.uv; o.color = v.color; o.slope = slope * (1 - v.color.r * 0.7);
                TRANSFER_SHADOW(o);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            // 21.271: 4 probe quanh camera do OpenWorldRiverReflections đưa vào, nội suy theo s (uv.x) của từng điểm nước.
            samplerCUBE _OWRefl0, _OWRefl1, _OWRefl2, _OWRefl3;
            float4 _OWReflPos0, _OWReflPos1, _OWReflPos2, _OWReflPos3;
            float4 _OWReflMin0, _OWReflMin1, _OWReflMin2, _OWReflMin3;
            float4 _OWReflMax0, _OWReflMax1, _OWReflMax2, _OWReflMax3;
            float4 _OWReflS; float _OWReflOn;

            float3 BoxDir(float3 r, float3 p, float3 c, float3 bmin, float3 bmax, float sea)
            {
                // 21.302c: điểm biển ngoài hộp probe → chiếu từ điểm kẹp trên mép hộp, càng xa hộp càng chuyển sang hướng phản
                // xạ thẳng (cảnh ở vô cực: trời/chân trời) — không tạo vệt hình quạt toả từ cửa sông.
                // sea = 0 với mặt sông (giữ nguyên cách cũ); biển = 1 − độ gần cửa sông (sát cửa sông chiếu như mặt sông)
                float3 q = sea > 0 ? lerp(p, clamp(p, bmin, bmax), sea) : p;
                float3 rr = r + (abs(r) < 1e-4) * 1e-4;
                float3 f = (rr > 0 ? (bmax - q) : (bmin - q)) / rr;
                float d = min(min(f.x, f.y), f.z);
                float3 dir = q + r * max(d, 0) - c;
                if (sea > 0) dir = lerp(normalize(dir), r, saturate(length(p - q) / 120) * sea);
                return dir;
            }

            // mouth (chỉ biển): 1 sát cửa sông → 0 xa 450 m (màu đỉnh a của lưới biển)
            float3 RiverReflect(float3 r, float3 p, float s, float mouth, float mip)
            {
                float4 S = _OWReflS; float4 w = 0;
                if (s <= S.x) w.x = 1;
                else if (s <= S.y) { float t = (s - S.x) / max(S.y - S.x, 1); w.x = 1 - t; w.y = t; }
                else if (s <= S.z) { float t = (s - S.y) / max(S.z - S.y, 1); w.y = 1 - t; w.z = t; }
                else if (s <= S.w) { float t = (s - S.z) / max(S.w - S.z, 1); w.z = 1 - t; w.w = t; }
                else w.w = 1;
                float sea = 0;
                if (_SeaMode > 0.5)
                {   // 21.302c: biển xa cửa sông trộn 4 probe theo khoảng cách ngang tới từng probe (liền mạch); sát cửa sông dùng đúng
                    // trọng số theo s như mặt sông (s của tim sông kéo dài ra biển) → không lộ đường ranh ở mép lưới sông
                    float4 d2 = float4(dot(p.xz - _OWReflPos0.xz, p.xz - _OWReflPos0.xz), dot(p.xz - _OWReflPos1.xz, p.xz - _OWReflPos1.xz),
                                       dot(p.xz - _OWReflPos2.xz, p.xz - _OWReflPos2.xz), dot(p.xz - _OWReflPos3.xz, p.xz - _OWReflPos3.xz));
                    float4 wi = 1 / (d2 + 3600); wi /= dot(wi, 1);
                    w = lerp(wi, w, mouth); sea = 1 - mouth;
                }
                float3 c = 0;
                if (w.x > 0) c += w.x * texCUBElod(_OWRefl0, float4(BoxDir(r, p, _OWReflPos0.xyz, _OWReflMin0.xyz, _OWReflMax0.xyz, sea), mip)).rgb;
                if (w.y > 0) c += w.y * texCUBElod(_OWRefl1, float4(BoxDir(r, p, _OWReflPos1.xyz, _OWReflMin1.xyz, _OWReflMax1.xyz, sea), mip)).rgb;
                if (w.z > 0) c += w.z * texCUBElod(_OWRefl2, float4(BoxDir(r, p, _OWReflPos2.xyz, _OWReflMin2.xyz, _OWReflMax2.xyz, sea), mip)).rgb;
                if (w.w > 0) c += w.w * texCUBElod(_OWRefl3, float4(BoxDir(r, p, _OWReflPos3.xyz, _OWReflMin3.xyz, _OWReflMax3.xyz, sea), mip)).rgb;
                return c;
            }

            float3 ProbeReflect(float3 r, float3 wpos, float mip)
            {
                float3 r0 = r;
                #if UNITY_SPECCUBE_BOX_PROJECTION
                    r0 = BoxProjectedCubemapDirection(r, wpos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
                #endif
                float3 c0 = DecodeHDR(UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, r0, mip), unity_SpecCube0_HDR);
                #if UNITY_SPECCUBE_BLENDING
                    float blend = unity_SpecCube0_BoxMin.w;
                    if (blend < 0.99999)
                    {
                        float3 r1 = r;
                        #if UNITY_SPECCUBE_BOX_PROJECTION
                            r1 = BoxProjectedCubemapDirection(r, wpos, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
                        #endif
                        float3 c1 = DecodeHDR(UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(unity_SpecCube1, unity_SpecCube0, r1, mip), unity_SpecCube1_HDR);
                        c0 = lerp(c1, c0, blend);
                    }
                #endif
                return c0;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float t = _Time.y;
                float dist = distance(_WorldSpaceCameraPos, i.world);
                float near = saturate(1 - dist / 140);
                float2 flow = i.color.gb * 2 - 1; float fl = length(flow); flow = fl > 0.1 ? flow / fl : float2(0, 1);
                float2 side = float2(-flow.y, flow.x);
                // 2 lớp trôi theo dòng (toạ độ dọc/ngang tim sông) + 1 lớp gió theo toạ độ thế giới
                float2 uv = i.flowUV;
                float2 a = (uv + float2(-t * _FlowSpeed, 0)) / _Tiling.x;
                float2 b = (float2(uv.x * 0.8 + uv.y * 0.6, uv.y * 0.8 - uv.x * 0.6) + float2(-t * _FlowSpeed * 1.35, t * 0.12)) / _Tiling.y;
                float2 c = (i.world.xz + float2(t * 0.35, -t * 0.22)) / _Tiling.z;
                float3 n1 = UnpackNormal(tex2D(_WaveNormal, a));
                float3 n2 = UnpackNormal(tex2D(_WaveNormal, b));
                float3 n3 = UnpackNormal(tex2D(_WaveNormal, c));
                // n1/n2 trong khung (dòng, ngang); n3 trong khung thế giới (x, z)
                float2 gFlow = (n1.xy * 0.6 + n2.xy * 0.4);
                float2 g = flow * gFlow.x + side * gFlow.y + n3.xy * (0.35 * near);
                g *= _NormalStrength * lerp(0.45, 1, near);
                if (_SeaMode > 0.5) { float2 sl; OWWaveHeight(i.world.xz, t, sl); g += sl * (1 - i.color.r * 0.7); }   // 21.302c
                else g += i.slope;
                float3 n = normalize(float3(-g.x, 1, -g.y));

                float3 v = normalize(_WorldSpaceCameraPos - i.world);
                float3 l = normalize(_WorldSpaceLightPos0.xyz);
                UNITY_LIGHT_ATTENUATION(atten, i, i.world);
                float vis = lerp(1 - _ShadowStrength, 1, atten);

                // phản chiếu: probe nướng sẵn; hướng phản xạ không cho chúi xuống dưới mặt nước
                float3 r = reflect(-v, n); r.y = max(r.y, 0.02);
                float mip = _ReflectBlur + (1 - near) * 1.5;
                // biển: s theo tim sông kéo dài qua cửa sông gần nhất (nam: nhánh dọc x 317,25; tây: nhánh ngang z 314,25 —
                // khớp OpenWorldRiverReflections.SOf); mặt sông: s = uv.x
                float sRef = i.flowUV.x, mouth = 0;
                if (_SeaMode > 0.5)
                {
                    bool south = distance(i.world.xz, float2(317.25, -600)) < distance(i.world.xz, float2(-600, 314.25));
                    sRef = south ? (1634.5 + 129.98 + (231.5 - i.world.z)) : (i.world.x + 1400);
                    mouth = i.color.a;
                }
                float3 refl = (_OWReflOn > 0.5 ? RiverReflect(r, i.world, sRef, mouth, mip) : ProbeReflect(r, i.world, mip)) * _ReflectTint.rgb;
                refl *= lerp(0.75, 1, vis);
                float ndv = saturate(dot(n, v));
                float fres = 0.02 + 0.98 * pow(1 - ndv, 5);
                float rw = lerp(_MinReflect, _MaxReflect, saturate(fres * 1.6));

                // thân nước: xanh rêu thẫm, loang theo gợn lớn; sát kè ngả màu bùn
                float mott = tex2D(_Foam, i.flowUV / 61 + float2(-t * 0.004, 0)).r;
                fixed3 body = lerp(_DeepColor.rgb, _MidColor.rgb, saturate(mott * 1.2 + n1.x * 0.5));
                body = lerp(body, _ShoreColor.rgb, i.color.r * 0.8);
                fixed3 amb = ShadeSH9(float4(0, 1, 0, 1));
                float ndl = saturate(dot(n, l));
                fixed3 lit = body * (amb * 0.85 + _LightColor0.rgb * (0.25 + 0.35 * ndl) * vis);
                fixed3 col = lerp(lit, refl, rw);

                // nắng: vệt sáng rộng + lấp lánh nhỏ trên đỉnh gợn
                float3 hv = normalize(l + v);
                float nh = saturate(dot(n, hv));
                float sun = pow(nh, 220) * _SunSpec + pow(nh, 1800) * _Sparkle * (0.5 + n2.x * n3.y * 6) * near;
                col += _LightColor0.rgb * sun * vis * saturate(l.y * 4);

                // bọt vỗ kè: dải hẹp sát chân kè, nhịp vỗ theo thời gian, rách theo texture bọt
                float shore = i.color.r;
                float pulse = 0.5 + 0.5 * sin(t * 1.4 + i.flowUV.x * 0.09);
                float foamTex = tex2D(_Foam, i.flowUV / 3.4 + float2(-t * 0.18, t * 0.05)).r;
                float band = smoothstep(0.80 - pulse * 0.08, 0.97, shore);
                float foam = saturate(band * (foamTex * 1.6 + 0.15)) * _ShoreFoam;
                col = lerp(col, _FoamColor.rgb * (amb * 0.6 + _LightColor0.rgb * 0.5 * vis), saturate(foam));

                UNITY_APPLY_FOG(i.fogCoord, col);
                return fixed4(col, 1);
            }
            ENDCG
        }
        // Bóng đổ nhận được cần mặt nước có trong ảnh độ sâu → pass ShadowCaster (dời đỉnh như pass chính).
        Pass
        {
            Tags { "LightMode"="ShadowCaster" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            #include "OpenWorldRiverWaves.cginc"
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; fixed4 color : COLOR; };
            struct v2f { V2F_SHADOW_CASTER; };
            v2f vert(appdata v)
            {
                v2f o;
                float3 w = mul(unity_ObjectToWorld, v.vertex).xyz; float2 s;
                w.y += OWWaveHeight(w.xz, _Time.y, s) * (1 - v.color.r * 0.7);
                v.vertex = mul(unity_WorldToObject, float4(w, 1));
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }
            float4 frag(v2f i) : SV_Target { SHADOW_CASTER_FRAGMENT(i) }
            ENDCG
        }
    }
    Fallback Off
}
