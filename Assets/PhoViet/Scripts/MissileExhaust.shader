Shader "PhoViet/Missile Exhaust"
{
 SubShader
 {
  Tags {"Queue"="Transparent" "RenderType"="Transparent"}
  Blend SrcAlpha One
  ZWrite Off
  Cull Off
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct a{float4 vertex:POSITION;float3 normal:NORMAL;fixed4 color:COLOR;};
   struct v{float4 pos:SV_POSITION;fixed4 color:COLOR;float3 normal:TEXCOORD0;float3 eye:TEXCOORD1;};
   v vert(a i){v o;o.pos=UnityObjectToClipPos(i.vertex);o.color=i.color;o.normal=UnityObjectToWorldNormal(i.normal);o.eye=_WorldSpaceCameraPos-mul(unity_ObjectToWorld,i.vertex).xyz;return o;}
   fixed4 frag(v i):SV_Target{float edge=pow(abs(dot(normalize(i.normal),normalize(i.eye))),1.7);return fixed4(i.color.rgb*1.7,i.color.a*edge*.65);}
   ENDCG
  }
 }
}
