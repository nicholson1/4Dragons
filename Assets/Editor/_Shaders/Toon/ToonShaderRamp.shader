Shader "Hidden/ToonShadingPostRamp"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _RampTex ("Ramp Texture", 2D) = "white" {}
        _Threshold ("Edge Threshold", Float) = 0.2
        _RampStrength ("Ramp Strength", Range(0,1)) = 1.0 // ADDED
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
            sampler2D _RampTex;
            float4 _MainTex_TexelSize;
            float _Threshold;
            float _RampStrength; // ADDED

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert(appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            float3 SampleRamp(float3 col)
            {
                float luminance = dot(col, float3(0.299, 0.587, 0.114));
                return tex2D(_RampTex, float2(saturate(luminance), 0)).rgb;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 col = tex2D(_MainTex, i.uv).rgb;

                // Edge detection
                float3 colN = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y)).rgb;
                float3 colE = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, 0)).rgb;
                float diff = length(col - colN) + length(col - colE);
                float edge = step(_Threshold, diff);

                // Ramp shading
                float3 toonColor = SampleRamp(col);

                // Blend between original and ramp (0 = none, 1 = full toon)
                float3 finalColor = lerp(col, toonColor, _RampStrength);

                return float4(finalColor * (1 - edge), 1.0);
            }
            ENDHLSL
        }
    }
}