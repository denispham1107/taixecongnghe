Shader "PhoViet/Morning Volume Fog"
{
    Properties { _MainTex("Scene",2D)="white"{} _DensityAtlas("Blender density volume",2D)="gray"{} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex,_DensityAtlas;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float4 _MainTex_TexelSize,_FogParams,_Atlas,_Period,_FogColor;
            float3 _RayBL,_RayBR,_RayTL,_RayTR,_CameraPosition;
            float4x4 _WorldToField;
            float sampleVolume(float3 worldPoint)
            {
                float3 p=mul(_WorldToField,float4(worldPoint,1)).xyz;
                p.xz+=float2(.65,.22)*_FogParams.w;
                float layer=saturate(p.y/_Period.y)*(_Atlas.x-1),lo=floor(layer),hi=min(lo+1,_Atlas.x-1);
                float2 at=frac(p.xz/_Period.xz)*_Atlas.x+1.5;
                float2 size=_Atlas.yz*_Atlas.w;
                float2 uv0=(float2(fmod(lo,_Atlas.y),floor(lo/_Atlas.y))*_Atlas.w+at)/size;
                float2 uv1=(float2(fmod(hi,_Atlas.y),floor(hi/_Atlas.y))*_Atlas.w+at)/size;
                float density=lerp(tex2Dlod(_DensityAtlas,float4(uv0,0,0)).r,tex2Dlod(_DensityAtlas,float4(uv1,0,0)).r,frac(layer));
                return density*exp(-max(0,p.y)*_FogParams.y)*(1-smoothstep(_Period.y*.66,_Period.y,p.y));
            }
            fixed4 frag(v2f_img i):SV_Target
            {
                float2 depthUV=i.uv;
                #if UNITY_UV_STARTS_AT_TOP
                if(_MainTex_TexelSize.y<0)depthUV.y=1-depthUV.y;
                #endif
                float depth=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,depthUV));
                float3 ray=lerp(lerp(_RayBL,_RayBR,depthUV.x),lerp(_RayTL,_RayTR,depthUV.x),depthUV.y);
                float lengthRay=length(ray),distance=min(_FogParams.z,depth*lengthRay),step=distance/6;
                float3 direction=ray/max(lengthRay,.0001);
                // Stable interleaved jitter avoids visible bands with six mobile-friendly samples.
                float jitter=.35+.3*frac(52.9829189*frac(dot(i.pos.xy,float2(.06711056,.00583715))));
                float opticalDepth=0;
                [unroll]for(int s=0;s<6;s++)opticalDepth+=sampleVolume(_CameraPosition+direction*((s+jitter)*step));
                float transmission=exp(-opticalDepth*step*_FogParams.x);
                fixed4 source=tex2D(_MainTex,i.uv);
                return fixed4(source.rgb*transmission+_FogColor.rgb*(1-transmission),source.a);
            }
            ENDCG
        }
    }
    Fallback Off
}
