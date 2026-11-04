Shader "PhoViet/Rotor Blur"
{
 SubShader
 {
  Tags {"Queue"="Transparent" "RenderType"="Transparent"}
  Pass
  {
   Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #include "UnityCG.cginc"
   struct v2f{float4 pos:SV_POSITION;fixed4 col:COLOR;UNITY_FOG_COORDS(0)};
   v2f vert(appdata_full v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.col=v.color;UNITY_TRANSFER_FOG(o,o.pos);return o;}
   fixed4 frag(v2f i):SV_Target{fixed4 c=i.col;UNITY_APPLY_FOG(i.fogCoord,c);return c;}
   ENDCG
  }
 }
}
