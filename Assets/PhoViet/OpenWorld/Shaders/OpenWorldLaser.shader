// Bản đồ mở Phố Việt (21.300): tia LAZER của xe người chơi (thân tia + chớp đầu đèn + chớp chỗ trúng). Ảnh dựng bằng Blender
// (ArtSource/Blender/OpenWorld/ow_laser_fx.py, nền đen) → cộng sáng: đen = không cộng. Màu = ảnh × _Color (HDR) × màu đỉnh
// (alpha đỉnh = độ sáng). Thân tia: _MainTex_ST (MaterialPropertyBlock) lặp ảnh theo chiều dài + trượt offset → năng lượng chạy.
Shader "PhoViet/OpenWorld/Laser"
{
    Properties
    {
        _MainTex ("Ảnh tia (Blender)", 2D) = "black" {}
        [HDR] _Color ("Màu", Color) = (1.6, 1.1, 1.1, 1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent+10" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend One One
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float4 _MainTex_ST; half4 _Color;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = TRANSFORM_TEX(v.uv, _MainTex); o.color = v.color; return o; }
            half4 frag (v2f i) : SV_Target
            {
                half3 c = tex2D(_MainTex, i.uv).rgb * _Color.rgb * i.color.rgb * i.color.a;
                return half4(c, 1);
            }
            ENDCG
        }
    }
}
