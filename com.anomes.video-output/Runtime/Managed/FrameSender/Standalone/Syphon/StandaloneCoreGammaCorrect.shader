Shader "UVO/Standalone/CoreGammaCorrect"
{
    Properties
    {
        _MainTex("", 2D) = ""{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;

    half4 frag_encode(v2f_img i) : SV_Target
    {
        half4 c = tex2D(_MainTex, i.uv);
        c.rgb = LinearToGammaSpace(c.rgb);
        return c;
    }

    ENDCG

    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode off }
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_encode
            ENDCG
        }
    }
}
