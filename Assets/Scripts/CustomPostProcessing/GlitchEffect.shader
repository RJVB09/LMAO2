Shader "Hidden/LMAO2/GlitchEffect"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float _Amount;
        float _PixelSize;

        #include "Noise.hlsl"

        float sampleNoise(float2 pixelizedCoords)
        {
            return _Amount * (Perlin(float3(pixelizedCoords * float2(10, 100),10+_Time.y*20),1,1) + 0.5 * Perlin(float3(pixelizedCoords * float2(100, 300),10+_Time.y*700),1,1)) > 0.5 ? 1 : 0;
        }

        float sampleNoiseLines(float2 pixelizedCoords)
        {
            return _Amount * (Perlin(float3(pixelizedCoords * float2(1, 300),10+_Time.y*20),1,1) + 0.5 * Perlin(float3(pixelizedCoords * float2(100, 300),10+_Time.y*700),1,1)) > 0.95 ? 1 : 0;
        }

        float sampleNoiseBackground(float2 pixelizedCoords)
        {
            return _Amount * (Perlin(float3(pixelizedCoords * float2(300, 300),10+_Time.y*800),1,1) + 0.5 * Perlin(float3(pixelizedCoords * float2(600, 600),10+_Time.y*700),1,1));
        }

        float4 Frag(VaryingsDefault i) : SV_Target
        {
            
            
            float pixelSize = 0.01;
            float2 pixelizedCoords = _PixelSize != 0 ? floor(i.texcoord / _PixelSize) * _PixelSize / (1 - _PixelSize) : i.texcoord;
            float noise = sampleNoise(pixelizedCoords);

            float shiftingAmount = 0.005;

            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pixelizedCoords);

            float4 colorShifted = float4(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pixelizedCoords + float2(-1,1) * shiftingAmount).r,SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pixelizedCoords - float2(-1,1) * shiftingAmount).gb,color.a);

            color = lerp(color, colorShifted, noise);

            //color.rgb *= sampleNoiseLines(pixelizedCoords) == 1 ? 2 : 1;
            color.rgb += sampleNoiseLines(pixelizedCoords) == 1 ? 0.04 : 0;

            color.rgb -= sampleNoiseBackground(pixelizedCoords).xxx * 0.001 * _Amount;


            return color;
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment Frag

            ENDHLSL
        }
    }
}