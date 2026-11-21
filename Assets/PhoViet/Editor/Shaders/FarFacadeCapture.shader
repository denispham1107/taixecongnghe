// 21.294 — chỉ dùng trong Editor: shader thay thế khi chụp mặt tiền nhà phố cho hộp ở xa. Ra màu nền (albedo) + phát sáng
// của vật liệu Triplanar, không ánh sáng/sương mù → ảnh chụp là màu gốc, ánh sáng do shader FarFacade tính lúc chơi.
Shader "Hidden/PhoViet/FarFacadeCapture"
{
    Properties
    {
        _Color ("Màu", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _TileH ("", Float) = 4
        _TileV ("", Float) = 4
        _Sharp ("", Float) = 6
        _EmissionColor ("", Color) = (0,0,0,1)
        _Glossiness ("", Float) = 0.3
        _Metallic ("", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; fixed4 _Color, _EmissionColor; float _TileH, _TileV, _Sharp, _Glossiness, _Metallic;
            struct v2f { float4 pos : SV_POSITION; float3 wp : TEXCOORD0; float3 wn : TEXCOORD1; };
            v2f vert (appdata_base v)
            {
                v2f o; o.pos = UnityObjectToClipPos(v.vertex);
                o.wp = mul(unity_ObjectToWorld, v.vertex).xyz; o.wn = UnityObjectToWorldNormal(v.normal); return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float3 n = abs(normalize(i.wn));
                float3 w = pow(n, max(_Sharp, 1)); w /= (w.x + w.y + w.z + 1e-5);
                float3 p = i.wp; float th = max(_TileH, 0.01), tv = max(_TileV, 0.01);
                fixed4 cx = tex2D(_MainTex, float2(p.z / th, p.y / tv));
                fixed4 cy = tex2D(_MainTex, float2(p.x / th, p.z / th));
                fixed4 cz = tex2D(_MainTex, float2(p.x / th, p.y / tv));
                fixed3 a = (cx * w.x + cy * w.y + cz * w.z).rgb * _Color.rgb;
                // kính/kim loại bóng: lúc chơi sáng nhờ phản chiếu trời → cộng xấp xỉ phản chiếu (màu trời) theo độ bóng
                fixed3 spec = lerp(fixed3(0.04, 0.04, 0.04), a, _Metallic) * _Glossiness * _Glossiness;
                fixed3 c = a * (1 - 0.5 * _Metallic) + spec * fixed3(0.62, 0.76, 0.9) * 2.6 + _EmissionColor.rgb;
                return fixed4(saturate(c), 1);
            }
            ENDCG
        }
    }
}
