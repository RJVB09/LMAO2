Shader "ImageEffect/Static"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseOpacity ("NoiseOpacity", float) = 0.1
        _DistortionLines ("DistortionLines", float) = 0
        _DistortionGlobal ("DistortionGlobal", float) = 0
        _Tint ("Tint",Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float3 pcg3d(int3 v) 
			{
                v = v * 1664525u + 1013904223u;

                v.x += v.y * v.z;
                v.y += v.z * v.x;
                v.z += v.x * v.y;

                v ^= v >> 16u;

                v.x += v.y * v.z;
                v.y += v.z * v.x;
                v.z += v.x * v.y;

                return v / 2147483648.0;
            }

            float Perlin(float3 pos, float amp, float freq)
            {
                if (amp == 0)
                    return 0;
                pos = mad(pos, float3(0.3, 0.3, 0.3), float3(100, 100, 100)) * freq;
                float3 final = 0;
                for (int i = 0; i < 8; i++)
                {
                    int3 r = int3(i / 4u, (i / 2u) % 2u, i % 2u);
                    int3 s = int3(pos)+r;
                    final += pcg3d(s) * saturate(1 - distance(r, frac(pos)));
                }
                return clamp(final.y - final.z + final.x, -2, 2) * 0.5 * amp;
            }
            
            float2 Perlin2D(float3 pos, float amp, float freq)
            {
                if (amp == 0)
                    return 0;
                pos = mad(pos, float3(0.3, 0.3, 0.3), float3(100, 100, 100)) * freq;
                float3 final = 0;
                for (int i = 0; i < 8; i++)
                {
                    int3 r = int3(i / 4u, (i / 2u) % 2u, i % 2u);
                    int3 s = int3(pos)+r;
                    final += pcg3d(s) * saturate(1 - distance(r, frac(pos)));
                }
                return final.xy * 0.5 * amp;
            }

            sampler2D _MainTex;
            float _NoiseOpacity;
            float _DistortionLines;
            float _DistortionGlobal;
            float4 _Tint;

            fixed4 frag (v2f i) : SV_Target
            {
                //float4 col = tex2D(_MainTex, i.uv) + float4(float3(0.1,0.1,0.1)*pcg3d(int3(i.uv*float2(26.354,12341),_Time.y*1000)).x,0);
                float4 noise = float4(float3(_NoiseOpacity,_NoiseOpacity,_NoiseOpacity)*Perlin(float3(i.uv*float2(35,1504),_SinTime.w*1000),1,1),0); //stripey noise thing
                float2 distortion = Perlin2D(float3(i.uv*float2(25,625),10+_Time.y*100),1,1); //weee wooo 
                float distortionLinesMask = max((Perlin(float3(4,i.uv.y*100+_Time.y*20,0),1,1)-0.5)*2+_DistortionLines,0);


                float4 col = tex2D(_MainTex, i.uv + 0.07 * (distortionLinesMask + _DistortionGlobal) * distortion) * _Tint + noise;
                //float4 col = distortionLinesMask;
                return col;
            }
            ENDCG
        }
    }
}
