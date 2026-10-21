Shader "PhoViet/Optical Light"
{
    Properties { _MainTex("Blender optical profile",2D)="black"{} _Tint("Tint",Color)=(1,1,1,1) _ProjectHeadlight("Project from animated lamp",Float)=0 }
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
            struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
            struct v2f {float4 position:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;UNITY_FOG_COORDS(2)};
            v2f vert(appdata v){v2f o;o.position=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.world=mul(unity_ObjectToWorld,v.vertex).xyz;UNITY_TRANSFER_FOG(o,o.position);return o;}
            fixed4 frag(v2f i):SV_Target
            {
                float2 uv=i.uv;float gain=1;
                if(_ProjectHeadlight>.5)
                {
                    float3 ray=mul(_WorldToHead,float4(i.world,1)).xyz;
                    // Blender reference: lamp at 1.1 m; cookie covers x [-16,16], z [-4,28].
                    // Reproject its rays from the live lamp so wheelies lift the beam off
                    // the road and landing pitch moves the footprint back toward the bike.
                    clip(-ray.y-.001);
                    float t=1.1/-ray.y;
                    uv=(ray.xz*t+float2(16,4))/32;
                    clip(min(uv,1-uv));
                    float distanceSquared=dot(ray,ray);
                    clip(32*32-distanceSquared);
                    float incidence=max(0,_HeadPosition.y-i.world.y)*t/1.1;
                    gain=min(1.5,t*t*incidence)*saturate((32-sqrt(distanceSquared))/6);
                }
                fixed4 c=fixed4(tex2D(_MainTex,uv).rgb*_Tint.rgb*_Tint.a*_PVNightLights*gain,0);
                UNITY_APPLY_FOG_COLOR(i.fogCoord,c,fixed4(0,0,0,0));return c;
            }
            ENDCG
        }
    }
}
