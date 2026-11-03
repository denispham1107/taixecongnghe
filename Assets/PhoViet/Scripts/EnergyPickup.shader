Shader "PhoViet/Energy Pickup"
{
 SubShader
 {
  Tags {"RenderType"="Opaque" "Queue"="Geometry"}
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #include "UnityCG.cginc"
   #include "Lighting.cginc"
   struct v2f {float4 pos:SV_POSITION;float3 normal:TEXCOORD0;float3 view:TEXCOORD1;fixed4 color:COLOR;UNITY_FOG_COORDS(2)};
   v2f vert(appdata_full v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);o.view=_WorldSpaceCameraPos-mul(unity_ObjectToWorld,v.vertex).xyz;o.color=v.color;UNITY_TRANSFER_FOG(o,o.pos);return o;}
   fixed4 frag(v2f i):SV_Target
   {
    float3 n=normalize(i.normal),v=normalize(i.view),l=normalize(_WorldSpaceLightPos0.xyz);
    float lambert=max(0,dot(n,l)),rim=pow(1-saturate(dot(n,v)),3);
    float spec=pow(max(0,dot(n,normalize(v+l))),40)*.25;
    fixed4 c=fixed4(i.color.rgb*(.48+lambert*.6+i.color.a*.75)+rim*float3(.02,.18,.28)+spec,1);
    UNITY_APPLY_FOG(i.fogCoord,c);return c;
   }
   ENDCG
  }
 }
}
