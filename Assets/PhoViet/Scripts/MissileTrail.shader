Shader "PhoViet/Missile Fire Trail"
{
 Properties { _Phase ("Flame phase", Float) = 0 }
 SubShader
 {
  Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
  Blend SrcAlpha One
  ZWrite Off
  Cull Off
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   float _Phase;
   struct a {float4 vertex:POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;};
   struct v {float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;};
   v vert(a i){v o;o.vertex=UnityObjectToClipPos(i.vertex);o.color=i.color;o.uv=i.uv;return o;}
   float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float noise(float2 p){float2 a=floor(p),t=frac(p);t=t*t*(3-2*t);return lerp(lerp(hash(a),hash(a+float2(1,0)),t.x),lerp(hash(a+float2(0,1)),hash(a+1),t.x),t.y);}
   fixed4 frag(v i):SV_Target
   {
    float across=saturate(1-abs(i.uv.y*2-1));
    float turbulent=noise(float2(i.uv.x*22-_Phase*18,i.uv.y*5+_Phase*2));
    float edge=smoothstep(0,.52,across-(1-turbulent)*.3),core=pow(across,8);
    fixed3 color=lerp(i.color.rgb,fixed3(1,.95,.65),core*.45);
    return fixed4(color*1.5,i.color.a*edge*(.45+.55*turbulent)*.9);
   }
   ENDCG
  }
 }
}
