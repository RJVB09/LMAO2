Shader "LMAO2/Standard" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Tex", 2D) = "white" {}
        _BumpMap ("Bumpmap", 2D) = "bump" {}
		_PBR ("PBR", 2D) = "white" {} //Red: Smoothness, Green: Metallic, Blue: Height
		_Emission ("Emission", 2D) = "black" {}

		[HDR]_EmissionColor ("Emission Color", Color) = (1,1,1,1)

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags {"Queue" = "Geometry" "RenderType" = "Opaque" }
		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 3.0
		
		float PI = 3.14159265359;


		struct Input {
			float2 uv_MainTex;

			INTERNAL_DATA
		};


		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
		}


		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _PBR;
		sampler2D _Emission;

		half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		float4 _EmissionColor;

		#include "UnityCG.cginc"

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float4 finalTex = tex2D(_MainTex,IN.uv_MainTex);
			float3 finalNorm = UnpackNormal(tex2D(_BumpMap,IN.uv_MainTex));
			float3 pbr = clamp(tex2D(_PBR,IN.uv_MainTex),0,1);
			float3 emissive = clamp(tex2D(_Emission,IN.uv_MainTex),0,1);


			o.Albedo = finalTex.rgb * _Color;
			//o.Albedo = float3(uvX,0);
			//o.Albedo = wp;
			//o.Albedo = clamp(pbr.rgb,0,1);

			o.Normal = finalNorm;


			o.Emission = emissive * _EmissionColor;

            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic * pbr.g;
            o.Smoothness = _Glossiness * pbr.r;
            o.Alpha = finalTex.a;
		}


		ENDCG
	} 
	Fallback "Diffuse"
}