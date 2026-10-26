Shader "PhoViet/Urban Rider"
{
 Properties {
  _MainTex("Received colour",2D)="white"{}
  _BumpMap("Normal",2D)="bump"{}
  _Surface("Metallic smoothness",2D)="black"{}
  _RegionMask("Cloth R / paint G",2D)="black"{}
  _ShirtColor("Shirt",Color)=(1,1,1,1)
  _BikeColor("Body paint",Color)=(1,1,1,1)
  _EmissionMap("Lenses",2D)="black"{}
  _EmissionColor("Night lenses",Color)=(0,0,0,1)
 }
 SubShader {
  Tags {"RenderType"="Opaque"} LOD 200
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows
  #pragma target 3.0
  sampler2D _MainTex,_BumpMap,_Surface,_RegionMask,_EmissionMap;
  fixed4 _ShirtColor,_BikeColor,_EmissionColor;
  struct Input {float2 uv_MainTex;};
  void surf(Input IN,inout SurfaceOutputStandard o) {
   fixed3 mask=tex2D(_RegionMask,IN.uv_MainTex).rgb;
   o.Albedo=tex2D(_MainTex,IN.uv_MainTex).rgb*lerp(1,_ShirtColor.rgb,mask.r)*lerp(1,_BikeColor.rgb,mask.g);
   o.Normal=UnpackNormal(tex2D(_BumpMap,IN.uv_MainTex));
   fixed4 surface=tex2D(_Surface,IN.uv_MainTex);
   o.Metallic=surface.r;o.Smoothness=surface.a*.32;
   o.Emission=tex2D(_EmissionMap,IN.uv_MainTex).rgb*_EmissionColor.rgb;o.Alpha=1;
  }
  ENDCG
 }
 Fallback "Standard"
}
