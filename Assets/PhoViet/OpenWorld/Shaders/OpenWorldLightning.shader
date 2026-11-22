// Bản đồ mở Phố Việt (21.317): tia sét dựng bằng Blender (Resources/ThoiTiet/PV_Set.fbx, build_mua_set.py).
// Mỗi nhánh là dải 2 đỉnh/điểm nằm trên đường tâm; shader nở dải vuông góc hướng nhìn camera (luôn thấy đủ bề rộng từ mọi
// phía). Bề rộng tối thiểu theo khoảng cách để tia ở xa không mảnh hơn ~1,5 điểm ảnh.
// UV0 = (bên −1/+1, thời điểm tới t), UV1 = (hướng.x, hướng.y), UV2 = (hướng.z, bề rộng tương đối), UV3 = (độ sáng nhánh, 0..1 dọc nhánh).
// Hai pass cộng sáng: quầng xanh tím rộng (ánh sáng tán trong mưa) + lõi trắng nóng mảnh.
// _Reveal: đầu tia dẫn đường đi xuống (0 → 1); _Flash: độ sáng chung; _Branch: độ sáng nhánh so với kênh chính (nhánh tắt nhanh hơn).
Shader "PhoViet/OpenWorld/Lightning"
{
    Properties
    {
        [HDR] _CoreColor ("Lõi", Color) = (2.6, 2.75, 3.2, 1)
        [HDR] _GlowColor ("Quầng", Color) = (0.42, 0.5, 1.15, 1)
        _CoreWidth ("Bề rộng lõi (m)", Float) = 0.55
        _GlowWidth ("Bề rộng quầng (m)", Float) = 7
        _MinPix ("Bề rộng tối thiểu theo khoảng cách", Float) = 0.0035
        _Reveal ("Đầu tia dẫn đường", Float) = 1.2
        _Flash ("Độ sáng", Float) = 1
        _Branch ("Độ sáng nhánh", Float) = 1
        _Seed ("Hạt ngẫu nhiên", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+20" "RenderType"="Transparent" "IgnoreProjector"="True" "DisableBatching"="True" }
        Blend One One
        ZWrite Off
        Cull Off

        CGINCLUDE
        #include "UnityCG.cginc"
        half4 _CoreColor, _GlowColor; float _CoreWidth, _GlowWidth, _MinPix, _Reveal, _Flash, _Branch, _Seed;
        struct appdata { float4 vertex : POSITION; float2 uv0 : TEXCOORD0; float2 uv1 : TEXCOORD1; float2 uv2 : TEXCOORD2; float2 uv3 : TEXCOORD3; };
        struct v2f { float4 pos : SV_POSITION; float3 d : TEXCOORD0; };       // d = (bên, độ sáng, nhấp nháy)

        v2f expand(appdata v, float width, float minScale)
        {
            v2f o;
            float3 wp = mul(unity_ObjectToWorld, v.vertex).xyz;
            float3 dir = normalize(mul((float3x3)unity_ObjectToWorld, float3(v.uv1.x, v.uv1.y, v.uv2.x)) + 1e-5);
            float3 toCam = _WorldSpaceCameraPos - wp; float dist = length(toCam);
            float3 side = cross(dir, toCam / max(dist, 1e-3));
            side = side / max(length(side), 1e-3);
            float w = max(width * v.uv2.y, _MinPix * minScale * dist);
            wp += side * v.uv0.x * w * 0.5;
            o.pos = mul(UNITY_MATRIX_VP, float4(wp, 1));
            float t = v.uv0.y;
            // đầu tia dẫn đường: phần đã đi qua hiện mờ, sát đầu sáng hơn; phần chưa tới thì tắt
            float shown = step(t, _Reveal);
            float head = saturate(1 - (_Reveal - t) * 8);
            float isMain = step(0.99, v.uv3.x);
            float bright = lerp(v.uv3.x * _Branch * (1 - 0.65 * v.uv3.y), 1, isMain);
            bright *= lerp(0.35 + 0.65 * head, 1, step(1.0, _Reveal));       // đang dẫn đường: mờ trừ đầu tia
            // nhấp nháy dọc kênh (dòng điện chập chờn), khác nhau giữa các tia
            float flick = 0.82 + 0.18 * sin(t * 211 + _Seed * 13 + _Time.y * 97);
            // đoạn tia sát camera (tia đánh ngay cạnh/sau xe) mờ đi để quầng không trùm kín màn hình
            float nearFade = saturate((dist - 6) / 22);
            o.d = float3(v.uv0.x, bright * shown * _Flash * nearFade, flick);
            return o;
        }
        ENDCG

        Pass    // quầng sáng
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            v2f vert (appdata v) { return expand(v, _GlowWidth, 2.5); }
            half4 frag (v2f i) : SV_Target
            {
                float x = i.d.x; float k = exp(-x * x * 6.5) * 0.32;
                return half4(_GlowColor.rgb * k * i.d.y, 1);
            }
            ENDCG
        }
        Pass    // lõi trắng nóng
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            v2f vert (appdata v) { return expand(v, _CoreWidth, 1); }
            half4 frag (v2f i) : SV_Target
            {
                float x = abs(i.d.x); float k = saturate(1 - x); k = k * k * (3 - 2 * k);
                return half4(_CoreColor.rgb * k * i.d.y * i.d.z, 1);
            }
            ENDCG
        }
    }
}
