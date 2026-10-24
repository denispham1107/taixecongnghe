Shader "PhoViet/Day Night Sky"
{
    Properties { _Zenith("Zenith",Color)=(.7,.85,1,1) _Horizon("Horizon",Color)=(.95,.98,1,1) _Night("Night",Range(0,1))=0 _CloudCover("Cloud cover",Range(0,1))=.5 _CloudScale("Cloud scale",Float)=1.15 _CloudSpeed("Cloud drift",Float)=.006 _SkyBlue("Day sky blue",Color)=(.33,.6,.93,.78) }
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
            float4 _Zenith,_Horizon,_SkyBlue;float _Night,_CloudCover,_CloudScale,_CloudSpeed;float3 _SunDirection,_MoonDirection;float4x4 _SkyFrame;
            struct v2f {float4 vertex:SV_POSITION;float3 direction:TEXCOORD0;};
            v2f vert(float4 vertex:POSITION){v2f o;o.vertex=UnityObjectToClipPos(vertex);o.direction=vertex.xyz;return o;}
            float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
            float hash2(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float noise2(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(hash2(i),hash2(i+float2(1,0)),f.x),lerp(hash2(i+float2(0,1)),hash2(i+float2(1,1)),f.x),f.y);}
            // Mây tích kiểu hoạt hình như ảnh mẫu phố Sài Gòn: fBm 4 tầng trên mặt phẳng trời,
            // dày ở gần chân trời, thưa ở đỉnh; bám _SkyFrame nên không xoay theo mỗi lần rẽ.
            float clouds(float3 d,out float shade)
            {
                float h=d.y+.22;shade=0;if(d.y<=-.02)return 0;
                float2 uv=d.xz/h*_CloudScale+float2(_Time.y*_CloudSpeed,_Time.y*_CloudSpeed*.4);
                float n=0,a=.55;float2 q=uv;
                for(int k=0;k<4;k++){n+=noise2(q)*a;q=q*2.07+float2(13.1,7.7);a*=.5;}
                float lower=noise2(uv*2.07+float2(13.1,7.7)-float2(0,.35))*.5;
                float band=smoothstep(.0,.08,d.y)*(1-smoothstep(.35,.85,d.y));
                float m=smoothstep(1-_CloudCover,1.045-_CloudCover,n)*band;
                shade=saturate(smoothstep(1-_CloudCover,1.25-_CloudCover,n)*.75+.25-lower*.25);
                return m;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float3 d=normalize(i.direction);float up=saturate(d.y);
                float3 c=lerp(_Horizon.rgb,_Zenith.rgb,pow(up,.55));
                // Ban ngày trời cao xanh đậm như ảnh mẫu, chân trời vẫn sáng nhạt; đêm giữ màu profile.
                c=lerp(c,_SkyBlue.rgb,smoothstep(.02,.55,up)*_SkyBlue.a*(1-_Night));
                float sunDot=saturate(dot(d,normalize(_SunDirection)));
                c+=float3(1,.96,.9)*(pow(sunDot,650)*.65+pow(sunDot,14)*.08)*(1-_Night);
                float moonDot=saturate(dot(d,normalize(_MoonDirection)));
                c+=float3(.64,.78,1)*(smoothstep(.9991,.99965,moonDot)*.7+pow(moonDot,200)*.035)*_Night;
                float3 starDirection=mul((float3x3)_SkyFrame,d);float3 cell=floor(starDirection*260);float star=step(.9984,hash(cell))*pow(saturate(sin(starDirection.x*16900)*sin(starDirection.y*17400)),12);
                c+=star*smoothstep(.65,1,_Night)*smoothstep(.05,.35,up)*.65;
                float3 cloudDirection=mul((float3x3)_SkyFrame,d);if(dot(cloudDirection.xz,cloudDirection.xz)<1e-6)cloudDirection=d;cloudDirection.y=d.y;float shade;
                float cloud=clouds(normalize(cloudDirection),shade);
                float3 dayCloud=lerp(float3(.7,.77,.88),float3(1,1,1),shade);float3 nightCloud=lerp(float3(.07,.09,.14),float3(.2,.24,.33),shade);
                c=lerp(c,lerp(dayCloud,nightCloud,_Night),cloud*.96);
                return fixed4(c,1);
            }
            ENDCG
        }
    }
}
