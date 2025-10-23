Shader "Hidden/WhiteAlphaFromLuminance"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        ZTest Always Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float _Threshold;   // small cutoff to remove tiny noise
            float _KeepColor;   // 0 = output pure white, 1 = keep original RGB

            struct VIn  { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct VOut { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            VOut Vert(VIn v)
            {
                VOut o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                return o;
            }

            half4 Frag(VOut i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // Alpha from brightness
                half a = max(col.r, max(col.g, col.b));
                a = a > _Threshold ? a : 0;

                // Output color
                half3 outRgb = lerp(half3(1,1,1), col.rgb, _KeepColor);

                return half4(outRgb, a);
            }
            ENDHLSL
        }
    }
}
