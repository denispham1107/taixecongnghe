// Nền thành phố (24/10/2026): Standard như vật liệu cũ, nhưng bỏ qua điểm ảnh có stencil = 1
// do PhoViet/River Ground Mask ghi trên lòng sông, nhờ đó thấy mặt nước dưới cầu.
Shader "PhoViet/City Ground"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.12
        _Metallic ("Metallic", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Stencil { Ref 1 Comp NotEqual }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        fixed4 _Color; half _Glossiness,_Metallic;
        struct Input { float3 worldPos; };
        void surf(Input IN,inout SurfaceOutputStandard o)
        { o.Albedo=_Color.rgb; o.Metallic=_Metallic; o.Smoothness=_Glossiness; o.Alpha=1; }
        ENDCG
    }
    FallBack "Diffuse"
}
