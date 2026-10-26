// Lớp nước đục dưới mặt sông (24/10/2026): vài mặt phẳng trong suốt xếp theo độ sâu, vẽ từ sâu lên
// trước mặt nước. Vật chìm càng sâu nằm sau càng nhiều lớp nên mờ dần vào màu sông. Chỉ vẽ khi có vật đang chìm (21.54).
Shader "PhoViet/River Murk"
{
    Properties
    {
        _Color ("Murk", Color) = (0.05,0.30,0.38,0.42)
    }
    SubShader
    {
        Tags { "Queue"="Transparent-2" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            fixed4 _Color;float _RiverMurkOn;   // PhoVietRiver.UpdateRiverMurk: 1 khi có người/xe đang chìm (21.54)
            struct v2f { float4 pos:SV_POSITION; fixed3 light:COLOR; UNITY_FOG_COORDS(1) };
            v2f vert(appdata_base v)
            {
                v2f o;o.pos=UnityObjectToClipPos(v.vertex);
                // Không có gì dưới nước: đẩy đỉnh ra ngoài khung cắt để GPU bỏ cả 4 lớp (không tô, không trộn). Khi đó các lớp
                // nằm sau mặt nước đục ≥ 93% nên đóng góp < 1% màu — tắt đi không thấy khác (21.54).
                if(_RiverMurkOn<0.5)o.pos=float4(3,3,3,1);
                // Theo ánh sáng môi trường (ngày sáng, đêm tối) để nước sâu không phát sáng ban đêm.
                o.light=saturate(ShadeSH9(float4(0,1,0,1))*1.1);
                UNITY_TRANSFER_FOG(o,o.pos);return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=fixed4(_Color.rgb*i.light,_Color.a);
                UNITY_APPLY_FOG(i.fogCoord,c);
                return c;
            }
            ENDCG
        }
    }
}
