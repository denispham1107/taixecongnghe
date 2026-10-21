Shader "PhoViet/Day Night Sky"
{
    Properties { _Zenith("Zenith",Color)=(.7,.85,1,1) _Horizon("Horizon",Color)=(.95,.98,1,1) _Night("Night",Range(0,1))=0 }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 _Zenith,_Horizon;float _Night;float3 _SunDirection,_MoonDirection;float4x4 _SkyFrame;
            struct v2f {float4 vertex:SV_POSITION;float3 direction:TEXCOORD0;};
            v2f vert(float4 vertex:POSITION){v2f o;o.vertex=UnityObjectToClipPos(vertex);o.direction=vertex.xyz;return o;}
            float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
            fixed4 frag(v2f i):SV_Target
            {
                float3 d=normalize(i.direction);float up=saturate(d.y);
                float3 c=lerp(_Horizon.rgb,_Zenith.rgb,pow(up,.55));
                float sunDot=saturate(dot(d,normalize(_SunDirection)));
                c+=float3(1,.96,.9)*(pow(sunDot,650)*.65+pow(sunDot,14)*.08)*(1-_Night);
                float moonDot=saturate(dot(d,normalize(_MoonDirection)));
                c+=float3(.64,.78,1)*(smoothstep(.9991,.99965,moonDot)*.7+pow(moonDot,200)*.035)*_Night;
                float3 starDirection=mul((float3x3)_SkyFrame,d);float3 cell=floor(starDirection*260);float star=step(.9984,hash(cell))*pow(saturate(sin(starDirection.x*16900)*sin(starDirection.y*17400)),12);
                c+=star*smoothstep(.65,1,_Night)*smoothstep(.05,.35,up)*.65;
                return fixed4(c,1);
            }
            ENDCG
        }
    }
}
