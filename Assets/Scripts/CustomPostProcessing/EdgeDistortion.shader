Shader "Hidden/LMAO2/EdgeDistortion"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float _RadialFreq;
        float _AngularFreq;
        float _TimeEvolution;
        float _Strength;
        float _Falloff;

        #include "Noise.hlsl"

        float4 Frag(VaryingsDefault i) : SV_Target
        {
            
            
            float theta = atan2(i.texcoord.x - 0.5f, i.texcoord.y - 0.5f);
            float r = length(i.texcoord - float2(0.5,0.5)) * 2;

            float2 distortion = Perlin2D(float3(float2(_RadialFreq * r,_AngularFreq * theta),10+_Time.y*_TimeEvolution),1,1);

            float2 rUnitVec = - normalize(i.texcoord - float2(0.5,0.5));
            float2 thetaUnitVec = float2(cos(theta), sin(theta));

            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + (distortion.x * rUnitVec + distortion.y * thetaUnitVec) * _Strength * pow(r, _Falloff));

            //color.rgb = distortion.xyy;


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