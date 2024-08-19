Shader "Custom/XRay"
{
    Properties {
        _FillColor ("Fill Color", Color) = (0,0,0,1)
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

        void surf (Input IN, inout SurfaceOutput o)
		{
			float2 uv = IN.screenPos.xy / IN.screenPos.w;
            //float rim = pow(max(0,2*(saturate(dot (normalize(IN.viewDir), IN.vertexNormal))-0.5)),_Pow) * _Precense;
            float rim = 1;

            /*
			#if UNITY_UV_STARTS_AT_TOP
				grabTexcoord.y = 1.0f - grabTexcoord.y;
			#else
				grabTexcoord.y = grabTexcoord.y;
			#endif
            */

			float3 c = tex2D(_GrabTexture, uv);
            float n = (c.r + c.g + c.b) / 3;
            n = pow((sin(10 * 3.14159265359 * n)+1)/2,100);
            c = float3(0,n,0);
            //c = float3(rim,rim,rim);
            //c = IN.vertexNormal;
			
            o.Emission = c.rgb;
			o.Albedo = float3(0, 0, 0);
            o.Alpha = 1;
        }
        ENDCG
    }
}
