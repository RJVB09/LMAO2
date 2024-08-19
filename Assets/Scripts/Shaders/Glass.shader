// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Glass"
{
    Properties {
        _Color ("Color", Color) = (0,0,0,1)
        _Refraction ("Refraction", float) = 0.1
    }
    SubShader {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent"}
        LOD 200
		Lighting Off

        GrabPass { }

		CGPROGRAM
        #pragma surface surf Lambert alpha vertex:vert

        sampler2D _GrabTexture;
        half4 _Color;
        float _Refraction;

        struct Input {
            float4 screenPos;
            float3 vertexNormal;
            float3 viewDir;
            float3 worldPos;
        };

        void vert (inout appdata_full v, out Input o) {
           UNITY_INITIALIZE_OUTPUT(Input,o);
           o.vertexNormal = v.normal;
        }

        float len(float3 a)
        {
            return pow(pow(a.x,3) + pow(a.y,3) + pow(a.z,3),0.333);
        }

        float project(float3 a, float3 b)
        {
            return len((dot(a,b)/dot(b,b))*b);
        }

        void surf (Input IN, inout SurfaceOutput o)
		{
			float2 uv = IN.screenPos.xy / IN.screenPos.w;
            //float3 viewDirLocalX = normalize(float3(IN.viewDir.z,0,-IN.viewDir.x));
            //float3 viewDirLocalY = cross(IN.viewDir,viewDirLocalX);
            //float2 projectedUV = float2(project(IN.vertexNormal,viewDirLocalX),project(IN.vertexNormal,viewDirLocalY));
            float2 projectedUV = UnityObjectToClipPos(float4(IN.worldPos + IN.vertexNormal - _WorldSpaceCameraPos, 1.0f));
            float2 projectedScreenUV = UnityObjectToClipPos(float4(IN.worldPos - _WorldSpaceCameraPos, 1.0f));
            float rim = 1.0 - saturate(dot (normalize(IN.viewDir), IN.vertexNormal));
            /*
			#if UNITY_UV_STARTS_AT_TOP
				grabTexcoord.y = 1.0f - grabTexcoord.y;
			#else
				grabTexcoord.y = grabTexcoord.y;
			#endif
            */

			float3 c = tex2D(_GrabTexture, uv - _Refraction*rim*(projectedUV));
            //c.rg = projectedUV;
            //c.b = 0;

            //c.rg = 2*(uv - float2(0.5,0.5));
            //c.b = 0;
            //c = float3(projectedUV.x,projectedUV.y,0);
            //c = IN.vertexNormal;
			
            o.Emission = c.rgb * _Color.rgb;
			o.Albedo = float3(0, 0, 0);
            o.Alpha = 1;
        }
        ENDCG
    } 

}
