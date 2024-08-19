Shader "Custom/Distort"
{
    Properties {
        _ReplaceColor ("Replace Color", Color) = (1,0,1,1)
        _FillColor ("Fill Color", Color) = (0,0,0,1)
        _T ("Time", float) = 0
        _Precense ("Precense",float) = 1
        _Pow ("RimPower",float) = 2
    }
    SubShader {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent"}
        LOD 200
		Lighting Off

        GrabPass { }
        

		CGPROGRAM
        #pragma surface surf Lambert alpha vertex:vert

        sampler2D _GrabTexture;
        float _T;
        float _Pow;
        float _Precense;
        half4 _ReplaceColor;
        half4 _FillColor;

        struct Input {
            float4 screenPos;
            float3 vertexNormal;
            float3 viewDir;
        };

        void vert (inout appdata_full v, out Input o) {
           UNITY_INITIALIZE_OUTPUT(Input,o);
           o.vertexNormal = v.normal;
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

        void surf (Input IN, inout SurfaceOutput o)
		{
			float2 uv = IN.screenPos.xy / IN.screenPos.w;
            float rim = pow(max(0,2*(saturate(dot (normalize(IN.viewDir), IN.vertexNormal))-0.5)),_Pow) * _Precense;
            
            float2 distortion = Perlin2D(float3(uv,10+_T*1),1,10); //weee wooo 
            float2 distortion2 = Perlin2D(float3(uv,10+_T*1.5),1,20); //weee wooo 
            float2 distortion3 = Perlin2D(float3(uv,10+_T*3),1,40); //weee wooo 
            float2 distortedUV = uv + 0.2 * rim * distortion + 0.3 * rim  * distortion2 + 0.6 * rim * distortion2;

            float blindSpots = Perlin(float3(distortedUV,_T*0.01),1,15); //weee wooo

            /*
			#if UNITY_UV_STARTS_AT_TOP
				grabTexcoord.y = 1.0f - grabTexcoord.y;
			#else
				grabTexcoord.y = grabTexcoord.y;
			#endif
            */

			float3 c = tex2D(_GrabTexture, distortedUV);
            c *= lerp(float3(1,1,1),float3(pow(sin(_T*0.1) + 1,0.5),pow(-sin(_T*0.1) + 1,0.5),pow(sin(_T*0.1) + 1,0.5)),rim);
            c -= lerp(float3(0,0,0),float3(0.439, 1, 0.494) * max(0,pow(1 - abs(blindSpots),101 * (pow(2,(sin(_T)+1)*0.5)-1))),rim);
            c = lerp(c,pow(clamp(c,0,1),2),rim);
            //c = float3(rim,rim,rim);
            //c = IN.vertexNormal;
			
            o.Emission = c.rgb;
			o.Albedo = float3(0, 0, 0);
            o.Alpha = 1;
        }
        ENDCG
    }
}
