// 21.272 — nội thất tầng trệt nhà phố (lưới Blender ow_interiors.py, màu vật liệu ghi vào màu đỉnh khi xuất):
//   color.rgb = màu gốc vật liệu Blender, color.a = độ phát sáng 0…1 (đèn trần, đèn đỏ bàn thờ, màn hình…)
// Trong nhà không có nắng trực tiếp: dùng đèn giả — sáng đều từ trần (mặt hướng lên/xuống sáng khác nhau), hắt nhẹ từ phía kính,
// tối dần về cuối phòng theo khoảng cách tới mặt tiền (toạ độ cục bộ x: 0 = mặt tiền, −8,2 = tường hậu). GPU instancing.
Shader "PhoViet/OpenWorld/Interior"
{
    Properties
    {
        _LightColor ("Màu đèn trong nhà", Color) = (1.0, 0.95, 0.86, 1)
        _Brightness ("Độ sáng", Range(0, 2)) = 1.0
        _BackDim ("Tối dần về cuối phòng", Range(0, 1)) = 0.28
        _Emission ("Cường độ phát sáng", Float) = 2.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            fixed4 _LightColor; float _Brightness, _BackDim, _Emission;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; fixed4 color : COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos : SV_POSITION; fixed4 color : COLOR; float3 n : TEXCOORD0; float3 lp : TEXCOORD1; UNITY_FOG_COORDS(2) };
            v2f vert(appdata v)
            {
                v2f o; UNITY_SETUP_INSTANCE_ID(v);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color; o.n = v.normal; o.lp = v.vertex.xyz;
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float3 n = normalize(i.n);
                // độ sâu trong phòng theo trục mặt tiền cục bộ (mặt tiền x = 0, tường hậu |x| = 8,2)
                float depth = saturate(abs(i.lp.x) / 8.2);
                float up = n.y, front = abs(n.x);
                float light = 0.62 + 0.22 * saturate(up) - 0.12 * saturate(-up) + 0.12 * front;
                light *= 1 - _BackDim * depth;
                fixed3 amb = ShadeSH9(float4(0, 1, 0, 1));
                fixed3 c = i.color.rgb * (_LightColor.rgb * light * _Brightness + amb * 0.25);
                c += i.color.rgb * i.color.a * _Emission;
                UNITY_APPLY_FOG(i.fogCoord, c);
                return fixed4(c, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
