// Nền thành phố (24/10/2026): Standard như vật liệu cũ, nhưng bỏ qua điểm ảnh có stencil = 1
// do PhoViet/River Ground Mask ghi trên lòng sông, nhờ đó thấy mặt nước dưới cầu.
// Bỏ hẳn khi đang render ảnh phản chiếu mặt sông (_RiverReflecting, 21.45): từ mặt nước không nhìn thấy mặt dưới tấm nền.
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
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        fixed4 _Color; half _Glossiness,_Metallic; float _RiverReflecting;
        struct Input { float3 worldPos; };
        #include "TankCraterMask.cginc"
        void surf(Input IN,inout SurfaceOutputStandard o)
        { clip(0.5-_RiverReflecting); PVCutTankCrater(IN.worldPos); o.Albedo=_Color.rgb; o.Metallic=_Metallic; o.Smoothness=_Glossiness; o.Alpha=1; }
        ENDCG
    }
    FallBack "Diffuse"
}
