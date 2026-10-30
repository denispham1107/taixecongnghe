Shader "PhoViet/Road With Tank Craters"
{
 Properties { _MainTex("Albedo",2D)="white"{} _Color("Color",Color)=(1,1,1,1) _Glossiness("Smoothness",Range(0,1))=.12 _Metallic("Metallic",Range(0,1))=0 }
 SubShader
 {
  Tags{"RenderType"="Opaque" "Queue"="Geometry"}
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows addshadow
  #pragma target 3.0
  #include "TankCraterMask.cginc"
  sampler2D _MainTex;fixed4 _Color;half _Glossiness,_Metallic;
  struct Input {float2 uv_MainTex;float3 worldPos;};
  void surf(Input IN,inout SurfaceOutputStandard o)
  {
   PVCutTankCrater(IN.worldPos);
   o.Albedo=tex2D(_MainTex,IN.uv_MainTex).rgb*_Color.rgb;o.Metallic=_Metallic;o.Smoothness=_Glossiness;o.Alpha=1;
  }
  ENDCG
 }
 FallBack "Standard"
}
