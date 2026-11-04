Shader "PhoViet/Missile Surface"
{
 Properties { _Metallic("Metal",Range(0,1))=.35 _Glossiness("Smoothness",Range(0,1))=.45 }
 SubShader
 {
  Tags {"RenderType"="Opaque"}
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows
  #pragma target 3.0
  struct Input {fixed4 color:COLOR;};
  half _Metallic,_Glossiness;
  void surf(Input IN,inout SurfaceOutputStandard o){o.Albedo=IN.color.rgb;o.Metallic=_Metallic;o.Smoothness=_Glossiness;o.Alpha=1;}
  ENDCG
 }
 Fallback "Diffuse"
}
