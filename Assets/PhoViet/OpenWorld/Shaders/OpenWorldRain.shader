// Bản đồ mở Phố Việt (21.317): mưa giông. Ảnh render trong Blender (build_mua_set.py), trắng + alpha.
// _Mode 0 = hạt (vệt mưa, giọt bắn, vết cháy sét): màu = ảnh × _Color × màu đỉnh (alpha hạt).
// _Mode 1 = màn mưa trên ống quanh camera (PV_Mua.fbx): ảnh màn mưa cuộn xuống theo _Speed, 2 lớp lệch tỉ lệ cho có chiều sâu,
//           nghiêng theo gió (_Slant), mờ ở đỉnh/đáy ống; × _Alpha (độ mưa).
// _Mode 2 = vết cháy sét (ảnh × _Color). _Mode 3 = vòm mây giông quanh camera (cầu lớn): xám đen phía trên, sát chân trời đúng
//           màu sương mù cảnh; mây cuộn (nhiễu fbm theo hướng nhìn) trôi theo gió, sáng bừng theo _Flash khi sét đánh.
// Có sương mù như cảnh để màn mưa xa hòa vào trời xám.
Shader "PhoViet/OpenWorld/Rain"
{
    Properties
    {
        _MainTex ("Ảnh", 2D) = "white" {}
        _Color ("Màu", Color) = (0.78, 0.83, 0.9, 0.6)
        _Mode ("0 hạt, 1 màn mưa, 2 vết cháy, 3 vòm mây", Float) = 0
        _Flash ("Chớp sét (vòm mây)", Float) = 0
        _Alpha ("Độ mưa", Range(0,1)) = 1
        _Speed ("Tốc độ cuộn", Float) = 3.4
        _Slant ("Nghiêng gió", Float) = 0.06
        _SwapUV ("Đổi trục ảnh (hạt kéo dài)", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+10" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Offset -1, -1
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            sampler2D _MainTex; float4 _MainTex_ST; half4 _Color; float _Mode, _Alpha, _Speed, _Slant, _SwapUV, _Flash;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; float h : TEXCOORD1; UNITY_FOG_COORDS(2) float3 wp : TEXCOORD3; };
            float hash2(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float vnoise(float2 p) { float2 i = floor(p), f = frac(p); f = f * f * (3 - 2 * f);
                return lerp(lerp(hash2(i), hash2(i + float2(1, 0)), f.x), lerp(hash2(i + float2(0, 1)), hash2(i + float2(1, 1)), f.x), f.y); }
            float fbm(float2 p) { return vnoise(p) * 0.55 + vnoise(p * 2.03 + 7.1) * 0.28 + vnoise(p * 4.1 + 3.3) * 0.17; }
            v2f vert (appdata v)
            {
                v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = TRANSFORM_TEX(v.uv, _MainTex); o.color = v.color;
                o.h = v.uv.y; o.wp = mul(unity_ObjectToWorld, v.vertex).xyz;                                   // ống: 0 đáy → 4 đỉnh (4 lần lặp)
                UNITY_TRANSFER_FOG(o, o.pos); return o;
            }
            half4 frag (v2f i) : SV_Target
            {
                half4 c;
                if (_Mode > 2.5)
                {   // vòm mây giông: không nhận sương mù (tự pha màu sương ở chân trời)
                    float3 d = normalize(i.wp - _WorldSpaceCameraPos);
                    float up = saturate(d.y);
                    float2 q = d.xz / (up + 0.12) * 0.9 + _Time.y * float2(0.035, 0.018);
                    float n = fbm(q * 1.6);
                    float3 deck = _Color.rgb * (0.72 + 0.55 * n);                       // mây dày tối, chỗ mỏng sáng hơn
                    float3 col = lerp(unity_FogColor.rgb, deck, smoothstep(0.0, 0.3, up));
                    col += _Flash * (0.35 + 0.65 * n) * float3(0.55, 0.62, 0.8);           // sét soi sáng chân mây
                    return half4(col, _Alpha);
                }
                if (_Mode > 1.5)
                {   // vết cháy sét: không dùng màu đỉnh
                    c = tex2D(_MainTex, i.uv) * _Color;
                }
                else if (_Mode < 0.5)
                {
                    c = tex2D(_MainTex, _SwapUV > 0.5 ? i.uv.yx : i.uv) * _Color * i.color;
                }
                else
                {
                    float2 uv = i.uv; uv.x += uv.y * _Slant;
                    half a1 = tex2D(_MainTex, float2(uv.x, uv.y + _Time.y * _Speed)).a;
                    half a2 = tex2D(_MainTex, float2(uv.x * 1.7 + 0.37, uv.y * 1.45 + _Time.y * _Speed * 1.3)).a;
                    float fade = saturate(i.h / 0.8) * saturate((4 - i.h) / 1.2);
                    c = half4(_Color.rgb, saturate(a1 + a2 * 0.6) * _Color.a * _Alpha * fade);
                }
                UNITY_APPLY_FOG(i.fogCoord, c);
                return c;
            }
            ENDCG
        }
    }
}
