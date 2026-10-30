Shader "PhoViet/Thunderstorm FX"
{
 Properties { _Color("Tint",Color)=(.7,.85,1,.6) _Mode("Rain/Bolt/Warning",Float)=0 _Progress("Warning",Range(0,1))=0 _DstBlend("Destination blend",Float)=10 }
 SubShader
 {
  Tags{"Queue"="Transparent+20" "RenderType"="Transparent"} Blend SrcAlpha [_DstBlend] ZWrite Off Cull Off
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #include "UnityCG.cginc"
   fixed4 _Color;float _Mode,_Progress;
   struct V{float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   struct F{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;UNITY_FOG_COORDS(1)};
   F vert(V v){F o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;UNITY_TRANSFER_FOG(o,o.vertex);return o;}
   float segment(float2 p,float2 a,float2 b){float2 d=b-a;return length(p-a-d*saturate(dot(p-a,d)/dot(d,d)));}
   fixed4 frag(F i):SV_Target
   {
    fixed4 c=i.color*_Color;
    if(_Mode<.5)c.a*=pow(saturate(1-abs(i.uv.x*2-1)),2)*saturate(1-abs(i.uv.y*2-1));
    else if(_Mode<1.5){float core=pow(saturate(1-abs(i.uv.y*2-1)),2);c.a*=core;c.rgb*=1+core*2;}
    else
    {
     float2 p=i.uv*2-1;float r=length(p),a=atan2(p.y,p.x);
     float ring=(1-smoothstep(.025,.065,abs(r-.86)))*(.45+.55*step(0,sin(a*12)));
     float inner=(1-smoothstep(.02,.045,abs(r-lerp(.18,.8,_Progress))))*.45;
     float d=min(segment(p,float2(.12,.58),float2(-.2,.03)),min(segment(p,float2(-.2,.03),float2(.2,.03)),segment(p,float2(.2,.03),float2(-.12,-.58))));
     c.a*=max(max(ring,inner),1-smoothstep(.035,.07,d));c.rgb=lerp(float3(1,.6,.08),float3(1,.95,.65),_Progress);
    }
    UNITY_APPLY_FOG(i.fogCoord,c);return c;
   }
   ENDCG
  }
 }
}
