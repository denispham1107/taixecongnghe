Shader "PhoViet/Meshy Car Paint"
{
    Properties
    {
        _MainTex ("Received base color",2D)="white"{}
        _BumpMap ("Received normal",2D)="bump"{}
        _MetallicGlossMap ("Received metallic/smoothness",2D)="black"{}
        _PaintMask ("Paintable panels",2D)="black"{}
        _PaintColor ("Spawn paint",Color)=(1,1,1,1)
        _EmissionMap ("Actual lamps",2D)="black"{}
        _EmissionColor ("Night lamp level",Color)=(0,0,0,1)
        _Color ("Color",Color)=(1,1,1,1)
    }
    SubShader
    {
        Tags {"RenderType"="Opaque"}
        LOD 250
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        sampler2D _MainTex,_BumpMap,_MetallicGlossMap,_PaintMask,_EmissionMap;
        fixed4 _PaintColor,_EmissionColor,_Color;
        struct Input {float2 uv_MainTex;};
        void surf(Input IN,inout SurfaceOutputStandard o)
        {
            fixed3 original=tex2D(_MainTex,IN.uv_MainTex).rgb;
            half mask=tex2D(_PaintMask,IN.uv_MainTex).r;
            half value=max(original.r,max(original.g,original.b));
            o.Albedo=lerp(original,_PaintColor.rgb*(.20+.80*sqrt(value)),mask)*_Color.rgb;
            o.Normal=UnpackNormal(tex2D(_BumpMap,IN.uv_MainTex));
            fixed4 surface=tex2D(_MetallicGlossMap,IN.uv_MainTex);
            o.Metallic=surface.r;o.Smoothness=min(surface.a,.75);
            o.Emission=tex2D(_EmissionMap,IN.uv_MainTex).rgb*_EmissionColor.rgb;o.Alpha=1;
        }
        ENDCG
    }
    FallBack "Standard"
}
