// 21.295 — màu đạn pháo xe tăng / đạn tàu (trước đó lưới PV_TankShell không có vật liệu → Default-Material trắng toát).
// Lưới không cần UV: tô theo trục dọc cục bộ y (mũi nhọn y = −0,46, đáy phẳng y = +0,46): ngòi nổ đồng thau, thân xanh
// rêu quân sự có vạch vàng, đai dẫn bằng đồng đỏ gần đáy, đáy thép tối với lõi vạch đường (tracer) cam nóng phát sáng.
Shader "PhoViet/OpenWorld/TankShell"
{
    Properties
    {
        _Body ("Thân", Color) = (0.27, 0.31, 0.19, 1)
        _Fuze ("Ngòi nổ", Color) = (0.62, 0.5, 0.24, 1)
        _Stripe ("Vạch nhận dạng", Color) = (0.86, 0.68, 0.12, 1)
        _Band ("Đai dẫn đồng", Color) = (0.72, 0.36, 0.2, 1)
        _Base ("Đáy", Color) = (0.17, 0.17, 0.17, 1)
        [HDR] _Tracer ("Lõi cháy đáy", Color) = (4, 1.6, 0.35, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard vertex:vert
        #pragma target 3.0
        fixed4 _Body, _Fuze, _Stripe, _Band, _Base; half4 _Tracer;
        struct Input { float3 op; float3 on; };
        void vert (inout appdata_full v, out Input o) { UNITY_INITIALIZE_OUTPUT(Input, o); o.op = v.vertex.xyz; o.on = v.normal; }
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float y = IN.op.y;
            fixed3 c = _Body.rgb; half metal = 0.15, gloss = 0.38;
            if (y < -0.33) { c = _Fuze.rgb; metal = 0.85; gloss = 0.6; }
            else if (y > -0.13 && y < -0.06) { c = _Stripe.rgb; }
            else if (y > 0.29 && y < 0.37) { c = _Band.rgb; metal = 0.9; gloss = 0.55; }
            else if (y >= 0.37) { c = _Base.rgb; metal = 0.6; gloss = 0.35; }
            // gờ nhỏ giữa các tấm: tối nhẹ ở mép vạch cho thấy khối
            o.Albedo = c;
            o.Metallic = metal;
            o.Smoothness = gloss;
            // lõi tracer: mặt đáy phẳng (pháp tuyến +y) cháy cam, mép đáy hắt nhẹ
            float baseFace = saturate((IN.on.y - 0.7) * 3.3) * step(0.43, y);
            float r = length(IN.op.xz);
            o.Emission = _Tracer.rgb * baseFace * saturate(1.15 - r * 7.0) + _Tracer.rgb * 0.06 * step(0.43, y);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
