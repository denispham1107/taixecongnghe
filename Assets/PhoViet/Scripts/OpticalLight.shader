Shader "PhoViet/Optical Light"
{
    Properties { _MainTex("Blender optical profile",2D)="black"{} _Tint("Tint",Color)=(1,1,1,1) _ProjectHeadlight("Project from animated lamp",Float)=0 _HeadlightWidth("Horizontal beam spread",Float)=1 _HeadlightHeight("Optical reference height",Float)=1.1 }
    SubShader
    {
        Tags { "Queue"="Transparent-20" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend One One
        ZWrite Off
        Cull Off
        Offset -1,-1
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            sampler2D _MainTex;float4 _Tint;float _PVNightLights;
            float _ProjectHeadlight;float4x4 _WorldToHead;float3 _HeadPosition;
            float _HeadlightWidth,_HeadlightHeight;
            struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
            struct v2f {float4 position:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;UNITY_FOG_COORDS(2)};
            v2f vert(appdata v){v2f o;o.position=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.world=mul(unity_ObjectToWorld,v.vertex).xyz;UNITY_TRANSFER_FOG(o,o.position);return o;}
            float3 projectBeam(float3 world,float4x4 worldToLamp,float3 source,float referenceHeight,float width)
            {
                float3 ray=mul(worldToLamp,float4(world,1)).xyz;
                clip(-ray.y-.001);
                float t=referenceHeight/-ray.y;
                float2 uv=(ray.xz*t/float2(width,1)+float2(16,4))/32;
                // Mirror one half of the authored profile for precisely symmetrical car edges.
                if(width>1.01)uv.x=.5+abs(uv.x-.5);
                clip(min(uv,1-uv));
                float distanceSquared=dot(ray,ray);clip(32*32-distanceSquared);
                float incidence=max(0,source.y-world.y)*t/referenceHeight;
                float gain=min(1.5,t*t*incidence)*saturate((32-sqrt(distanceSquared))/6);
                return tex2D(_MainTex,uv).rgb*gain;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float3 profile;
                if(_ProjectHeadlight>.5)
                {
                    // A single continuous fan: only its horizontal opening is wider on cars.
                    profile=projectBeam(i.world,_WorldToHead,_HeadPosition,_HeadlightHeight,_HeadlightWidth);
                }
                else profile=tex2D(_MainTex,i.uv).rgb;
                fixed4 c=fixed4(profile*_Tint.rgb*_Tint.a*_PVNightLights,0);
                UNITY_APPLY_FOG_COLOR(i.fogCoord,c,fixed4(0,0,0,0));return c;
            }
            ENDCG
        }
    }
}
