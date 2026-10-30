Shader "PhoViet/Tank Crater Surface"
{
 Properties { _Color("Color",Color)=(1,1,1,1) _UseVertexColor("Use mesh color",Range(0,1))=1 }
 SubShader
 {
  Tags{"RenderType"="Opaque" "Queue"="Geometry"}
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows addshadow
  #pragma target 3.0
  fixed4 _Color;half _UseVertexColor;
  float4x4 _PVCraterOwnerInverse;
  float4 _PVCraterClipBand; // near, far, bridge half width, enabled
  struct Input {float4 color:COLOR;float3 worldPos;};
  void surf(Input IN,inout SurfaceOutputStandard o)
  {
   if(_PVCraterClipBand.w>0)
   {
    float3 local=mul(_PVCraterOwnerInverse,float4(IN.worldPos,1)).xyz;
    clip(7-abs(local.x));
    if(local.z>_PVCraterClipBand.x&&local.z<_PVCraterClipBand.y)clip(_PVCraterClipBand.z-abs(local.x));
   }
   float3 p=mul(unity_WorldToObject,float4(IN.worldPos,1)).xyz*39;
   float grain=frac(sin(dot(floor(p),float3(12.9898,78.233,37.719)))*43758.5453);
   float aa=saturate(1-max(fwidth(p.x),fwidth(p.z))*.6);
   o.Albedo=lerp(half3(1,1,1),IN.color.rgb,_UseVertexColor)*_Color.rgb*lerp(1,lerp(.85,1.12,grain),aa);o.Metallic=0;o.Smoothness=.035;o.Alpha=1;
  }
  ENDCG
 }
 FallBack "Diffuse"
}
