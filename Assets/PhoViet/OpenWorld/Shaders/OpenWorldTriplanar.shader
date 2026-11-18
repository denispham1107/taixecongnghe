// Bản đồ mở Phố Việt (21.249): lưới xuất từ Blender chưa có UV nên texture nướng từ vật liệu thủ tục được
// chiếu theo tọa độ THẾ GIỚI trên ba mặt phẳng (triplanar). _TileH/_TileV = kích thước ô lặp (mét) theo
// chiều ngang/chiều đứng, khớp vùng đã nướng trong Blender (Data/materials.json).
Shader "PhoViet/OpenWorld/Triplanar"
{
    Properties
    {
        _Color ("Màu", Color) = (1,1,1,1)
        _MainTex ("Texture nướng", 2D) = "white" {}
        _TileH ("Ô lặp ngang (m)", Float) = 4
        _TileV ("Ô lặp đứng (m)", Float) = 4
        _Glossiness ("Độ bóng", Range(0,1)) = 0.3
        _Metallic ("Kim loại", Range(0,1)) = 0
        [HDR] _EmissionColor ("Phát sáng", Color) = (0,0,0,1)
        _Sharp ("Độ sắc chuyển mặt", Range(1,16)) = 6
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #pragma multi_compile_instancing      // 21.250: vẽ hàng loạt cùng mẫu (nhà, bảng, cột đèn) bằng GPU instancing
        sampler2D _MainTex;
        fixed4 _Color;
        half _Glossiness, _Metallic, _TileH, _TileV, _Sharp;
        fixed4 _EmissionColor;
        struct Input { float3 worldPos; float3 worldNormal; };
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 n = abs(normalize(IN.worldNormal));
            float3 w = pow(n, _Sharp); w /= (w.x + w.y + w.z + 1e-5);
            float3 p = IN.worldPos;
            fixed4 cx = tex2D(_MainTex, float2(p.z / _TileH, p.y / _TileV));
            fixed4 cy = tex2D(_MainTex, float2(p.x / _TileH, p.z / _TileH));
            fixed4 cz = tex2D(_MainTex, float2(p.x / _TileH, p.y / _TileV));
            fixed4 c = (cx * w.x + cy * w.y + cz * w.z) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Emission = _EmissionColor.rgb;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
