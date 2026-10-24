// Vòng cảnh báo đạn pháo dưới đất (25/10/2026, huongdan.md 21.38): kết cấu vòng đỏ render trong Blender
// (PV_TankFx_Warning.png) + đĩa đỏ đầy dần từ tâm theo _Progress (0 lúc bắn → 1 lúc đạn chạm đất), nhấp nháy nhanh dần.
Shader "PhoViet/Tank Warning"
{
    Properties
    {
        _MainTex ("Warning ring", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Fill ("Fill colour", Color) = (1,0.08,0.03,0.42)
        _Progress ("Progress", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+5" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Offset -2,-2
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            sampler2D _MainTex;fixed4 _Color,_Fill;float _Progress;
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; UNITY_FOG_COORDS(1) };
            v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord.xy;UNITY_TRANSFER_FOG(o,o.pos);return o;}
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 ring=tex2D(_MainTex,i.uv)*_Color;
                float d=distance(i.uv,float2(0.5,0.5))*2;
                float pulse=0.72+0.28*sin(_Time.y*(10+_Progress*26));
                float fill=(1-smoothstep(_Progress*0.86-0.04,_Progress*0.86,d))*step(d,0.9);
                fixed4 c=fixed4(_Fill.rgb,_Fill.a*fill*(0.55+0.45*_Progress));
                c=lerp(c,fixed4(ring.rgb,1),ring.a);c.a=max(c.a,ring.a)*pulse;
                UNITY_APPLY_FOG(i.fogCoord,c);
                return c;
            }
            ENDCG
        }
    }
}
