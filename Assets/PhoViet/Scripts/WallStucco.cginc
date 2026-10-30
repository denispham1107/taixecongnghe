#ifndef PHOVIET_WALL_STUCCO_INCLUDED
#define PHOVIET_WALL_STUCCO_INCLUDED
sampler2D _StuccoDetail;
half _StuccoScale,_StuccoStrength,_StuccoContrast,_WallSmoothness;
// Author-space coordinates keep the finish attached during scrolling and junction rebases.
void PVStuccoVertex(inout appdata_full v,out Input o)
{
    UNITY_INITIALIZE_OUTPUT(Input,o);
    float3 n=normalize(v.normal);
    float3 t=abs(n.x)>max(abs(n.y),abs(n.z))?float3(0,1,0):float3(1,0,0);
    t=normalize(t-n*dot(n,t));float3 b=cross(n,t);
    v.tangent=float4(t,1);
    o.uv_MainTex=v.texcoord.xy;
    o.stuccoUV=float2(dot(v.vertex.xyz,t),dot(v.vertex.xyz,b));
}
void PVApplyStucco(float2 paletteUV,float2 detailUV,inout SurfaceOutputStandard o)
{
    // The import postprocessor marks only plaster faces inside the same flat palette cell.
    // Unmarked lettering, glass, foliage, metal and props retain their original shading.
    if(frac(paletteUV.x*8)>0.70)
    {
        half4 d=tex2D(_StuccoDetail,detailUV*_StuccoScale);
        o.Albedo*=1+(d.r-.5h)*_StuccoContrast;
        half2 slope=(d.ag*2-1)*_StuccoStrength;
        o.Normal=half3(slope,sqrt(saturate(1-dot(slope,slope))));
        o.Metallic=0;o.Smoothness=_WallSmoothness;
    }
}
#endif
