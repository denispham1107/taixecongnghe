// Mặt sông dưới cầu (24/10/2026, dựng lại theo phản hồi người dùng): lưới mặt nước nhấp nhô thật
// (dịch đỉnh bằng tổng sóng trong hệ tọa độ khối cầu, nên sóng đi cùng khối khi phố cuộn), hơi trong
// để thấy chiều sâu qua các lớp nước đục, phản chiếu trời theo Fresnel, lấp lánh nắng, bọt sát kè/trụ.
// Phản chiếu dùng màu trời môi trường ngày–đêm. Bóng đổ của mặt cầu, trụ cầu và hai bờ kè lên mặt nước tính giải tích theo hướng nắng/trăng hiện tại
// (vật liệu trong suốt không nhận được shadow map của Unity built-in).
Shader "PhoViet/River Water"
{
    Properties
    {
        _ShallowColor ("Shallow", Color) = (0.16,0.58,0.66,1)
        _DeepColor ("Deep", Color) = (0.03,0.24,0.36,1)
        _SkyColor ("Sky reflection", Color) = (0.85,0.95,1,1)
        _FoamColor ("Foam", Color) = (0.94,0.98,1,1)
        _WaveHeight ("Wave height", Float) = 1
        _WaveSpeed ("Wave speed", Float) = 1
        _Alpha ("Surface opacity", Range(0,1)) = 0.78
        _Sparkle ("Sun sparkle", Float) = 1.6
        _ShadowStrength ("Shadow strength", Range(0,1)) = 0.62
        // Hình khối che nắng (tọa độ khối cầu): mặt cầu, trụ, mép hai bờ.
        _Deck ("Deck half-width, bottom, top", Vector) = (3.7,-0.9,1.15,0)
        _Banks ("Near z, far z, bank top", Vector) = (16.8,42,0.15,0)
        _Piers ("Pier z1, z2, half-depth, half-width", Vector) = (25.2,33.6,0.62,3.3)
        _WaterY ("Water level", Float) = -5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            fixed4 _ShallowColor,_DeepColor,_SkyColor,_FoamColor;
            float _WaveHeight,_WaveSpeed,_Alpha,_Sparkle,_ShadowStrength;
            float4 _Deck,_Banks,_Piers;float _WaterY;
            struct v2f { float4 pos:SV_POSITION; float3 obj:TEXCOORD0; float3 world:TEXCOORD1; UNITY_FOG_COORDS(2) };

            // Sóng chính: 4 hướng, bước sóng 7,4 / 4,1 / 2,5 / 1,5 m, vận tốc pha theo nước sâu (ω = √(g·k)).
            static const float2 D0=float2(0.97,0.24),D1=float2(-0.62,0.78),D2=float2(0.28,-0.96),D3=float2(-0.83,-0.56);
            float Height(float2 p,float t,out float2 slope)
            {
                float h=0;slope=0;
                float k,w,a,ph;
                k=0.85;w=sqrt(9.8*k);a=0.065;ph=dot(D0,p)*k+t*w;h+=a*sin(ph);slope+=D0*(a*k*cos(ph));
                k=1.53;w=sqrt(9.8*k);a=0.038;ph=dot(D1,p)*k+t*w;h+=a*sin(ph);slope+=D1*(a*k*cos(ph));
                k=2.51;w=sqrt(9.8*k);a=0.022;ph=dot(D2,p)*k+t*w;h+=a*sin(ph);slope+=D2*(a*k*cos(ph));
                k=4.19;w=sqrt(9.8*k);a=0.012;ph=dot(D3,p)*k+t*w;h+=a*sin(ph);slope+=D3*(a*k*cos(ph));
                h*=_WaveHeight;slope*=_WaveHeight;return h;
            }
            // Mesh cầu giữ trục Blender (Z lên) và renderer xoay -90° quanh X: đổi qua lại hệ khối phố (Y lên).
            float3 ToTile(float3 m){return float3(m.x,m.z,-m.y);}
            float3 FromTile(float3 t){return float3(t.x,-t.z,t.y);}
            float Hash(float2 p){p=frac(p*float2(123.34,456.21));p+=dot(p,p+45.32);return frac(p.x*p.y);}
            float Noise(float2 p)
            {
                float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),lerp(Hash(i+float2(0,1)),Hash(i+float2(1,1)),f.x),f.y);
            }
            v2f vert(appdata_base v)
            {
                v2f o;float2 slope;
                float3 p=ToTile(v.vertex.xyz);p.y+=Height(p.xz,_Time.y*_WaveSpeed,slope);
                float3 m=FromTile(p);
                o.pos=UnityObjectToClipPos(float4(m,1));o.obj=p;o.world=mul(unity_ObjectToWorld,float4(m,1)).xyz;
                UNITY_TRANSFER_FOG(o,o.pos);return o;
            }
            // Tia từ điểm mặt nước về phía nguồn sáng có cắt hộp không (phương pháp phiến).
            float HitBox(float3 o,float3 d,float3 lo,float3 hi)
            {
                float3 inv=1/(d+(abs(d)<1e-4)*1e-4);
                float3 t0=(lo-o)*inv,t1=(hi-o)*inv;
                float3 tmin=min(t0,t1),tmax=max(t0,t1);
                float enter=max(max(tmin.x,tmin.y),tmin.z),leave=min(min(tmax.x,tmax.y),tmax.z);
                return (leave>max(enter,0))?1:0;
            }
            float Shadow(float3 p,float3 l)
            {
                if(l.y<=0.02)return 1;
                float s=HitBox(p,l,float3(-_Deck.x,_Deck.y,_Banks.x),float3(_Deck.x,_Deck.z,_Banks.y));
                s=max(s,HitBox(p,l,float3(-_Piers.w,-12,_Piers.x-_Piers.z),float3(_Piers.w,_Deck.y,_Piers.x+_Piers.z)));
                s=max(s,HitBox(p,l,float3(-_Piers.w,-12,_Piers.y-_Piers.z),float3(_Piers.w,_Deck.y,_Piers.y+_Piers.z)));
                // Bờ kè: tia đi về phía bờ gần/xa mà còn thấp hơn mặt bờ là bị đất/kè che.
                if(l.z<-1e-3){float t=(_Banks.x-p.z)/l.z;if(p.y+t*l.y<_Banks.z)s=1;}
                if(l.z>1e-3){float t=(_Banks.y-p.z)/l.z;if(p.y+t*l.y<_Banks.z)s=1;}
                return s;
            }
            // Gợn nhỏ dạng nhiễu (không theo một hướng) để mặt nước không thành sọc; mờ dần theo khoảng cách.
            float RippleHeight(float2 p,float t)
            {
                return Noise(p*1.7+float2(t*0.35,t*0.22))*0.55+Noise(p*3.9-float2(t*0.5,-t*0.31))*0.3+Noise(p*8.3+float2(-t*0.8,t*0.6))*0.15;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float t=_Time.y*_WaveSpeed;float2 slope;
                Height(i.obj.xz,t,slope);
                float dist=distance(_WorldSpaceCameraPos,i.world);
                float detail=saturate(1-dist/55);
                const float e=0.06;
                float r0=RippleHeight(i.obj.xz,t),rx=RippleHeight(i.obj.xz+float2(e,0),t),rz=RippleHeight(i.obj.xz+float2(0,e),t);
                float2 g=slope*lerp(0.55,1,detail)+float2(rx-r0,rz-r0)/e*0.045*detail;
                float3 n=normalize(UnityObjectToWorldNormal(FromTile(normalize(float3(-g.x,1,-g.y)))));
                float3 v=normalize(_WorldSpaceCameraPos-i.world);
                float3 l=normalize(_WorldSpaceLightPos0.xyz);
                float3 lObj=normalize(ToTile(mul((float3x3)unity_WorldToObject,l)));
                // Bóng mềm của mặt cầu, trụ và bờ kè: trung bình 5 mẫu quanh điểm ảnh.
                float sh=Shadow(i.obj,lObj);
                sh+=Shadow(i.obj+float3(0.2,0,0),lObj)+Shadow(i.obj-float3(0.2,0,0),lObj)+Shadow(i.obj+float3(0,0,0.2),lObj)+Shadow(i.obj-float3(0,0,0.2),lObj);
                sh/=5;float lightVis=1-sh*_ShadowStrength;
                float ndv=saturate(dot(n,v));
                float fresnel=0.02+0.98*pow(1-ndv,5);
                fixed3 ambient=ShadeSH9(float4(0,1,0,1));
                float crest=saturate((i.obj.y-_WaterY)*7+0.35);
                float mottled=Noise(i.obj.xz*0.11+t*0.02)*0.65+Noise(i.obj.xz*0.37-t*0.03)*0.35;
                fixed3 body=lerp(_DeepColor.rgb,_ShallowColor.rgb,saturate(0.2+0.45*mottled+0.3*crest));
                float diffuse=saturate(dot(n,l)*0.6+0.4);
                fixed3 lit=body*(ambient*0.85+_LightColor0.rgb*0.55*diffuse*lightVis);
                // Ánh sáng xuyên qua đỉnh sóng (tán xạ dưới mặt).
                lit+=_ShallowColor.rgb*_LightColor0.rgb*saturate(dot(-v,l)+0.3)*crest*0.12*lightVis;
                // Trời phản chiếu theo màu môi trường ngày–đêm (thiên đỉnh/chân trời), không dùng cubemap trắng.
                float3 r=reflect(-v,n);
                fixed3 sky=lerp(unity_AmbientEquator.rgb*1.05,unity_AmbientSky.rgb*1.1,saturate(r.y*1.6))*_SkyColor.rgb;
                float reflectivity=saturate(fresnel)*0.55*lerp(0.75,1,lightVis);
                float3 h=normalize(l+v);
                float glint=pow(saturate(dot(n,h)),420)*_Sparkle*detail+pow(saturate(dot(n,h)),70)*0.08;
                glint*=lightVis*saturate(l.y*4);
                // Bọt trắng sát chân kè và quanh trụ cầu, đập theo sóng.
                float wall=min(i.obj.z-_Banks.x,_Banks.y-i.obj.z);
                float2 pz=float2(min(abs(i.obj.z-_Piers.x),abs(i.obj.z-_Piers.y))-_Piers.z,abs(i.obj.x)-_Piers.w);
                float pier=length(max(pz,0))+min(max(pz.x,pz.y),0);
                float edge=min(wall,pier+0.05);
                float foamNoise=Noise(i.obj.xz*2.3+float2(t*0.6,-t*0.4))*0.7+Noise(i.obj.xz*5.1-t*0.9)*0.3;
                float foam=saturate((1-smoothstep(0.05,0.9,edge))*(foamNoise*1.5-0.3+crest*0.5));
                foam=saturate(foam+saturate(crest*1.4-1.05)*Noise(i.obj.xz*3.7+t)*0.35);
                fixed3 c=lerp(lit,sky,reflectivity)+_LightColor0.rgb*glint;
                c=lerp(c,_FoamColor.rgb*(ambient+_LightColor0.rgb*0.5*lightVis),foam*0.8);
                float alpha=saturate(lerp(_Alpha,0.95,fresnel)+foam*0.35+glint);
                fixed4 col=fixed4(c,alpha);
                UNITY_APPLY_FOG(i.fogCoord,col);
                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}
