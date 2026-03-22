Shader "Hidden/VRPainting/BrushStamp"
{
    Properties { _MainTex("Brush Shape", 2D) = "white" {} _Color("Color", Color) = (1,0,0,1) }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Always ZWrite Off Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            fixed4    _Color;
            struct a2v { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
            v2f vert(a2v v) { v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 s = tex2D(_MainTex, i.uv);
                // Soft-circle falloff when no texture assigned (white tex)
                float d = length(i.uv - 0.5) * 2.0;
                float alpha = s.a * _Color.a * (1.0 - smoothstep(0.7, 1.0, d));
                return fixed4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }
}