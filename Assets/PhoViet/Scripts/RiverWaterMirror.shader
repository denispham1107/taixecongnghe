// Mặt sông kiểu gương theo ảnh mẫu người dùng (25/10/2026, huongdan.md 21.45): phản chiếu phẳng thật
// (trời, mây, kè đá, nhà, cầu, xe — texture _RiverReflectionTex do RiverReflection render), gợn nhỏ làm rung
// hình phản chiếu, thân nước xanh đen sâu, sát kè ngả màu bùn ô liu, bóng cầu/trụ/kè làm tối mặt nước, ánh nắng
// lấp lánh nhỏ, chỉ một đường nước vỗ mảnh sát kè (không còn mảng bọt trắng). Không có texture phản chiếu thì dùng màu trời môi trường.
Shader "PhoViet/River Water Mirror"
{
    Properties
    {
        _DeepColor ("Deep body", Color) = (0.015,0.075,0.12,1)
        _ShallowColor ("Lighter body", Color) = (0.04,0.15,0.2,1)
        _EdgeColor ("Muddy edge", Color) = (0.17,0.18,0.12,1)
        _ReflectTint ("Reflection tint", Color) = (0.68,0.79,0.88,1)
        _FoamColor ("Lapping line", Color) = (0.86,0.9,0.88,1)
        _Reflectivity ("Base reflectivity", Range(0,1)) = 0.5
        _Distort ("Reflection distortion", Float) = 0.05
        _Ripple ("Ripple strength", Float) = 0.075
        _EdgeWidth ("Muddy edge width", Float) = 2.2
        _WaveHeight ("Wave height", Float) = 0.35
        _WaveSpeed ("Wave speed", Float) = 0.7
        _Alpha ("Surface opacity", Range(0,1)) = 0.93
        _Sparkle ("Sun sparkle", Float) = 1.2
        _ShadowStrength ("Shadow strength", Range(0,1)) = 0.62
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
            fixed4 _DeepColor,_ShallowColor,_EdgeColor,_ReflectTint,_FoamColor;
            float _Reflectivity,_Distort,_Ripple,_EdgeWidth,_WaveHeight,_WaveSpeed,_Alpha,_Sparkle,_ShadowStrength;
            float4 _Deck,_Banks,_Piers;float _WaterY;
            sampler2D _RiverReflectionTex;float4 _RiverReflectionTex_TexelSize;float _RiverReflectionOn,_RiverReflecting;
            struct v2f { float4 pos:SV_POSITION; float3 obj:TEXCOORD0; float3 world:TEXCOORD1; float4 screen:TEXCOORD2; UNITY_FOG_COORDS(3) };

            // Sóng dài êm (sông lặng như ảnh mẫu): 4 hướng, biên độ nhỏ.
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
                o.screen=ComputeScreenPos(o.pos);
                UNITY_TRANSFER_FOG(o,o.pos);return o;
            }
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
                if(l.z<-1e-3){float t=(_Banks.x-p.z)/l.z;if(p.y+t*l.y<_Banks.z)s=1;}
                if(l.z>1e-3){float t=(_Banks.y-p.z)/l.z;if(p.y+t*l.y<_Banks.z)s=1;}
                return s;
            }
            // Gợn lăn tăn nhiều hướng, trôi chậm theo dòng (trục x của khối là chiều dòng chảy).
            float Ripple(float2 p,float t)
            {
                return Noise(p*float2(0.9,1.6)+float2(t*0.32,t*0.05))*0.55+Noise(p*float2(2.2,3.4)-float2(t*0.55,-t*0.12))*0.33+Noise(p*5.3+float2(-t*0.9,t*0.4))*0.12;
            }
            fixed4 frag(v2f i):SV_Target
            {
                // Không vẽ chính mặt nước vào ảnh phản chiếu của nó.
                if(_RiverReflecting>0.5)discard;
                float t=_Time.y*_WaveSpeed;float2 slope;
                Height(i.obj.xz,t,slope);
                float dist=distance(_WorldSpaceCameraPos,i.world);
                float detail=saturate(1-dist/80);
                const float e=0.11;
                float r0=Ripple(i.obj.xz,t),rx=Ripple(i.obj.xz+float2(e,0),t),rz=Ripple(i.obj.xz+float2(0,e),t);
                float2 g=slope+float2(rx-r0,rz-r0)/e*_Ripple*lerp(0.35,1,detail);
                float3 n=normalize(UnityObjectToWorldNormal(FromTile(normalize(float3(-g.x,1,-g.y)))));
                float3 v=normalize(_WorldSpaceCameraPos-i.world);
                float3 l=normalize(_WorldSpaceLightPos0.xyz);
                float3 lObj=normalize(ToTile(mul((float3x3)unity_WorldToObject,l)));
                float sh=Shadow(i.obj,lObj);
                sh+=Shadow(i.obj+float3(0.25,0,0),lObj)+Shadow(i.obj-float3(0.25,0,0),lObj)+Shadow(i.obj+float3(0,0,0.25),lObj)+Shadow(i.obj-float3(0,0,0.25),lObj);
                sh/=5;float vis=1-sh*_ShadowStrength;
                float ndv=saturate(dot(n,v));
                float fresnel=0.02+0.98*pow(1-ndv,5);
                float w=lerp(_Reflectivity,0.94,saturate(fresnel*2.5));
                // Phản chiếu: ảnh gương theo tọa độ màn hình, lệch theo pháp tuyến gợn (càng xa càng ít).
                fixed3 refl;
                if(_RiverReflectionOn>0.5)
                {
                    // Lệch theo gợn: ngang rung nhẹ, dọc kéo dài (hình phản chiếu trên nước thật bị kéo dọc).
                    float3 nv=mul((float3x3)UNITY_MATRIX_V,n);
                    float2 uv=i.screen.xy/i.screen.w+float2(nv.x,nv.z*1.6)*_Distort/(1+dist*0.02);
                    // Làm mềm (5 mẫu, nhòe theo chiều dọc nhiều hơn) như ảnh mẫu.
                    float2 px=_RiverReflectionTex_TexelSize.xy*float2(1.1,2.6);
                    refl=tex2D(_RiverReflectionTex,uv).rgb*0.32+(tex2D(_RiverReflectionTex,uv+float2(px.x,0)).rgb+tex2D(_RiverReflectionTex,uv-float2(px.x,0)).rgb)*0.14+(tex2D(_RiverReflectionTex,uv+float2(0,px.y)).rgb+tex2D(_RiverReflectionTex,uv-float2(0,px.y)).rgb)*0.2;
                }
                else
                {
                    float3 r=reflect(-v,n);
                    refl=lerp(unity_AmbientEquator.rgb*1.05,unity_AmbientSky.rgb*1.1,saturate(r.y*1.6));
                }
                refl*=_ReflectTint.rgb*lerp(1,0.86,sh);
                // Thân nước: xanh đen sâu, loang nhẹ; sát kè ngả màu bùn như bờ nông trong ảnh.
                fixed3 ambient=ShadeSH9(float4(0,1,0,1));
                float wall=min(i.obj.z-_Banks.x,_Banks.y-i.obj.z);
                float mott=Noise(i.obj.xz*0.13+t*0.015)*0.6+Noise(i.obj.xz*0.41-t*0.02)*0.4;
                fixed3 body=lerp(_DeepColor.rgb,_ShallowColor.rgb,mott*0.6);
                body=lerp(body,_EdgeColor.rgb,(1-smoothstep(0,_EdgeWidth,wall))*0.75);
                fixed3 lit=body*(ambient*0.9+_LightColor0.rgb*0.45*vis*saturate(l.y*2+0.2));
                fixed3 c=lerp(lit,refl,w);
                float3 hv=normalize(l+v);
                float glint=pow(saturate(dot(n,hv)),600)*_Sparkle*detail*vis*saturate(l.y*4);
                c+=_LightColor0.rgb*glint;
                // Đường nước vỗ mảnh sát chân kè và trụ cầu.
                float2 pz=float2(min(abs(i.obj.z-_Piers.x),abs(i.obj.z-_Piers.y))-_Piers.z,abs(i.obj.x)-_Piers.w);
                float pier=length(max(pz,0))+min(max(pz.x,pz.y),0);
                float edge=min(wall,pier+0.05);
                float lap=(1-smoothstep(0.02,0.25,edge))*(Noise(i.obj.xz*4+t*0.5)*0.6+0.4);
                c=lerp(c,_FoamColor.rgb*(ambient+_LightColor0.rgb*0.4*vis),lap*0.35);
                float alpha=saturate(_Alpha+fresnel*0.3+lap*0.2+glint);
                fixed4 col=fixed4(c,alpha);
                UNITY_APPLY_FOG(i.fogCoord,col);
                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}
