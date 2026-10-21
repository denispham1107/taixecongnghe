Shader "PhoViet/Optical Light"
{
    Properties { _MainTex("Blender optical profile",2D)="black"{} _Tint("Tint",Color)=(1,1,1,1) }
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
            struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
            struct v2f {float4 position:SV_POSITION;float2 uv:TEXCOORD0;UNITY_FOG_COORDS(1)};
            v2f vert(appdata v){v2f o;o.position=UnityObjectToClipPos(v.vertex);o.uv=v.uv;UNITY_TRANSFER_FOG(o,o.position);return o;}
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=fixed4(tex2D(_MainTex,i.uv).rgb*_Tint.rgb*_Tint.a*_PVNightLights,0);
                UNITY_APPLY_FOG_COLOR(i.fogCoord,c,fixed4(0,0,0,0));return c;
            }
            ENDCG
        }
    }
}
