// Bản đồ mở Phố Việt (21.299): ánh điện cộng sáng cho lưới điện CSGT (dây lưới, tia điện chạy quanh lưới, tia lửa xẹt).
// _Mode 0 = dải (LineRenderer: sáng ở giữa bề ngang, nhấp nháy chạy dọc dây); 1 = hạt tròn (tia lửa, quầng).
// Màu = _Color (HDR) × màu đỉnh (LineRenderer/hạt) — alpha đỉnh nhân độ sáng.
Shader "PhoViet/OpenWorld/Electric"
{
    Properties
    {
        [HDR] _Color ("Màu điện", Color) = (0.55, 0.9, 1.6, 1)
        _Mode ("0 dải, 1 hạt", Float) = 0
        _Flicker ("Nhấp nháy", Range(0,1)) = 0.35
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend One One
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            half4 _Color; float _Mode, _Flicker;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color; return o; }
            half4 frag (v2f i) : SV_Target
            {
                float k;
                if (_Mode < 0.5)
                {
                    float across = 1 - abs(i.uv.y * 2 - 1);
                    k = pow(saturate(across), 1.6);
                    k *= 1 - _Flicker + _Flicker * (0.5 + 0.5 * sin(i.uv.x * 37 - _Time.y * 41) * sin(i.uv.x * 11 + _Time.y * 17));
                }
                else
                {
                    float r = length(i.uv - 0.5) * 2;
                    k = pow(saturate(1 - r), 2.2);
                }
                return half4(_Color.rgb * i.color.rgb * i.color.a * k, 1);
            }
            ENDCG
        }
    }
}
