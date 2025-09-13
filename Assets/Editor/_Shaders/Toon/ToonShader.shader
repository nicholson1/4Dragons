Shader "Hidden/ToonShadingPost"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Threshold ("Edge Threshold", Float) = 0.2
        _Steps ("Color Steps", Float) = 4
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Threshold;
            float _Steps;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 col = tex2D(_MainTex, i.uv).rgb;

                // Posterize the color
                col = floor(col * _Steps) / _Steps;

                // Edge detection using depth/color difference
                float3 colN = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y)).rgb;
                float3 colE = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, 0)).rgb;
                float diff = length(col - colN) + length(col - colE);

                float edge = step(_Threshold, diff);

                return float4(col * (1 - edge), 1.0);
            }
            ENDHLSL
        }
    }
}