// Bản đồ mở Phố Việt (21.324): vết hỏng dán lên thân ô tô sau va chạm — xước tróc sơn (lớp lót + kim loại trần),
// kính nứt mạng nhện, đèn vỡ. Lưới dán dựng lúc chạy từ chính các tam giác thân xe quanh chỗ va (OpenWorldCarDent),
// đẩy ra 3 mm theo pháp tuyến + Offset để không nhấp nháy. Ảnh PV_XeHong_Atlas (4 ô) × màu đỉnh (alpha = độ mờ biên).
Shader "PhoViet/OpenWorld/CarDamage"
{
    Properties
    {
        _MainTex ("Atlas vết hỏng", 2D) = "white" {}
        _Glossiness ("Độ bóng", Range(0,1)) = 0.55
        _Metallic ("Kim loại", Range(0,1)) = 0.45
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest+60" "RenderType"="Transparent" "IgnoreProjector"="True" }
        ZWrite Off
        Offset -1, -2
        CGPROGRAM
        #pragma surface surf Standard alpha:fade nolightmap noshadow nodynlightmap nometa
        #pragma target 3.0
        sampler2D _MainTex; half _Glossiness, _Metallic;
        struct Input { float2 uv_MainTex; float4 color : COLOR; };
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb * IN.color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a * IN.color.a;
        }
        ENDCG
    }
    FallBack Off
}
