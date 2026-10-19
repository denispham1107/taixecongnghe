Shader "PhoViet/Waypoint"
{
 Properties { _Color ("Color", Color) = (0.16,1,0.12,1) _MainTex ("Texture", 2D) = "white" {} }
 SubShader
 {
  Tags { "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" }
  Blend SrcAlpha OneMinusSrcAlpha
  ZWrite Off ZTest Always Cull Off
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   fixed4 _Color;
   sampler2D _MainTex;
   struct input { float4 vertex:POSITION;float2 uv:TEXCOORD0; };
   struct output { float4 vertex:SV_POSITION;float2 uv:TEXCOORD0; };
   output vert(input v) { output o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o; }
   fixed4 frag(output i):SV_Target { fixed4 c=_Color;c.a*=tex2D(_MainTex,i.uv).a;return c; }
   ENDCG
  }
 }
}
