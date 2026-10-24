// Mặt nạ lòng sông (24/10/2026): vẽ trước nền thành phố, chỉ ghi stencil = 1 trên dải sông,
// để "Nền thành phố" (PhoViet/City Ground) không phủ màu lên mặt nước thấp hơn 2 m.
Shader "PhoViet/River Ground Mask"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }
        Pass
        {
            ColorMask 0
            ZWrite Off
            Stencil { Ref 1 Comp Always Pass Replace }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 vert(float4 v:POSITION):SV_POSITION { return UnityObjectToClipPos(v); }
            fixed4 frag():SV_Target { return 0; }
            ENDCG
        }
    }
}
