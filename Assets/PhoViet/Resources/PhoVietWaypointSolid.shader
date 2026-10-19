Shader "PhoViet/Waypoint Solid"
{
 Properties { _Color ("Color", Color) = (0.08,1,0.56,1) }
 SubShader
 {
  Tags { "Queue"="Overlay+1" "RenderType"="Opaque" "IgnoreProjector"="True" }
  ZWrite Off ZTest Always Cull Back
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   fixed4 _Color;
   struct input {float4 vertex:POSITION;float3 normal:NORMAL;};
   struct output {float4 vertex:SV_POSITION;float3 normal:TEXCOORD0;};
   output vert(input v){output o;o.vertex=UnityObjectToClipPos(v.vertex);o.normal=v.normal;return o;}
   fixed4 frag(output i):SV_Target
   {
    float3 n=normalize(i.normal),light=normalize(float3(-.45,.8,-1));
    float diffuse=.44+.56*saturate(dot(n,light));
    float specular=pow(saturate(dot(reflect(-light,n),float3(0,0,-1))),28)*.25;
    return fixed4(_Color.rgb*diffuse+specular,1);
   }
   ENDCG
  }
 }
}
