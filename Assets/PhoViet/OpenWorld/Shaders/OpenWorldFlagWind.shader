// Bản đồ mở Phố Việt (21.298): cờ đỏ sao vàng trên cột đèn bay nhè nhẹ trong gió (menu 19, PhoVietOpenWorldFlagBuilder).
// Tấm vải một lớp (vẽ hai mặt), mép sát cột + mép thanh treo trên giữ cố định, mép dưới/ngoài tự do. Sóng vải tính ở đỉnh:
//   uv.x = s (0 sát cột → 1 mép ngoài), uv.y = 1 − t (1 mép trên → 0 mép dưới); tangent = hướng s, pháp tuyến = mặt vải.
// Độ lệch d(s,t) theo pháp tuyến = 2 sóng chạy từ cột ra ngoài + 1 sóng chéo nhỏ + lắc chậm, nhân bao biên (0 ở cột/thanh).
// Pháp tuyến tính lại từ đạo hàm d để nếp vải có sáng tối thật. Nhịp lệch theo vị trí cột → các lá không bay đồng loạt.
Shader "PhoViet/OpenWorld/FlagWind"
{
    Properties
    {
        _MainTex ("Vải cờ", 2D) = "white" {}
        _Color ("Màu", Color) = (0.93,0.93,0.93,1)
        _Glossiness ("Độ bóng", Range(0,1)) = 0.22
        _Emission ("Tự sáng (đỏ tươi cả khi khuất nắng)", Range(0,1)) = 0.08
        _Size ("Rộng (m), dài (m)", Vector) = (1, 1.65, 0, 0)
        _Amp ("Biên độ sóng (m)", Float) = 0.22
        _Speed ("Tốc độ sóng", Float) = 2.9
        _WaveLen ("Bước sóng (m)", Float) = 0.8
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Cull Off
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        #pragma target 3.0
        #pragma multi_compile_instancing
        sampler2D _MainTex;
        fixed4 _Color;
        half _Glossiness, _Emission;
        float4 _Size;
        float _Amp, _Speed, _WaveLen;

        struct Input { float2 uv_MainTex; float facing : VFACE; };

        // độ lệch theo pháp tuyến (mét) tại (s, t) — s, t ∈ [0,1]
        float Disp(float s, float t, float time, float ph)
        {
            float x = s * _Size.x, y = t * _Size.y;
            float k = 6.2831853 / max(_WaveLen, 0.05);
            float w = sin(k * x - _Speed * time + ph) * 0.62
                    + sin(k * 1.7 * x + 2.3 * y - _Speed * 1.55 * time + ph * 1.7) * 0.24
                    + sin(0.9 * time + ph * 0.5 + y * 0.8) * 0.3;               // lắc chậm cả tấm
            float env = pow(saturate(s), 1.15) * saturate(t * _Size.y / 0.15) * (0.45 + 0.55 * t);
            return _Amp * w * env;
        }

        void vert (inout appdata_full v)
        {
            float s = v.texcoord.x, t = 1 - v.texcoord.y;
            float3 wp = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
            float ph = dot(wp.xz, float2(0.37, 0.23));
            float time = _Time.y;
            float3 n = normalize(v.normal), ts = normalize(v.tangent.xyz);
            float3 tt = normalize(cross(n, ts)) * v.tangent.w;                       // hướng t (xuống dưới)
            float d = Disp(s, t, time, ph);
            const float e = 0.01;
            float dds = (Disp(s + e, t, time, ph) - d) / (e * _Size.x);
            float ddt = (Disp(s, t + e, time, ph) - d) / (e * _Size.y);
            v.vertex.xyz += n * d - ts * abs(d) * 0.35;                                // vải co lại chút khi gợn
            v.normal = normalize(n - ts * dds - tt * ddt);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = 0;
            o.Smoothness = _Glossiness;
            o.Emission = c.rgb * _Emission;
            o.Normal = float3(0, 0, IN.facing > 0 ? 1 : -1);                          // mặt sau: lật pháp tuyến
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
