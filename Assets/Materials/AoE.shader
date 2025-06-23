Shader "Unlit/AoE"
{
    Properties
    {
        _Color("Color", Color) = (1, 0, 0, 0.5)
        _Smooth("Edge Smoothness", Range(0.001, 0.2)) = 0.05
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
        }
        LOD 100

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            fixed4 _Color;
            float _Smooth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv - 0.5; // center UVs so (0,0) is middle of mesh
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dist = length(i.uv);
                float alpha = 1.0 - smoothstep(0.5 - _Smooth, 0.5, dist);
                return fixed4(_Color.rgb, _Color.a * alpha);
            }
            ENDCG
        }
    }
}
