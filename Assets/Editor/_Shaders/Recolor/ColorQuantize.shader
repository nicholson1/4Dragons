Shader "Hidden/ColorQuantize"
{
    Properties {}
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            Name "Quantize"
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                return o;
            }

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _Palette[64];
            int _PaletteCount;

            float3 FindClosestColor(float3 color)
            {
                float minDist = 1e6;
                float3 best = color;
                for (int i = 0; i < _PaletteCount; i++)
                {
                    float3 p = _Palette[i].rgb;
                    float dist = distance(color, p);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        best = p;
                    }
                }
                return best;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                col.rgb = FindClosestColor(col.rgb);
                return col;
            }
            ENDHLSL
        }
    }
}