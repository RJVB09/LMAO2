Shader "LMAO2/Triplanar" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Tex", 2D) = "white" {}
        _BumpMap ("Bumpmap", 2D) = "bump" {}
		_PBR ("PBR", 2D) = "white" {} //Red: Smoothness, Green: Metallic, Blue: Height
		_Emission ("Emission", 2D) = "black" {}

		[HDR]_EmissionColor ("Emission Color", Color) = (1,1,1,1)

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_BorderFade ("BorderFade", Range(0,100)) = 10
		_TexScale ("Texture scaling", float) = 1
		_NormalStrength ("Normal strength", float) = 1
		_Offset ("Texture Offset", vector) = (0,0,0)
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 3.0
		
		float PI = 3.14159265359;


		struct Input {
			float3 vertexNormal;
			float3 vertexObjectNormal;
			float3 worldPos;
			float3 vertexWorldPos;
			INTERNAL_DATA
		};


		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.vertexObjectNormal = mul(unity_ObjectToWorld,v.normal);
			o.vertexNormal = v.normal;
			o.vertexWorldPos = v.vertex;
		}


		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _PBR;
		sampler2D _Emission;

		half _Glossiness;
        half _Metallic;
		half _BorderFade;
        fixed4 _Color;
		float4 _EmissionColor;
		float _TexScale;
		float _NormalStrength;
		float3 _Offset;

		#include "UnityCG.cginc"
		#include "TriplanarDependencies.cginc"

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float3 worldScale = float3(
				length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x)), // scale x axis
				length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y)), // scale y axis
				length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z))  // scale z axis
			);

			float3 wp = (abs((IN.vertexWorldPos.xyz+_Offset) * worldScale.xyz * _TexScale)) % 1;
			float3 vn = IN.vertexNormal.xyz;

			wp.x = flip(wp.x,(IN.vertexWorldPos.xyz+_Offset).x);
			wp.y = flip(wp.y,(IN.vertexWorldPos.xyz+_Offset).y);
			wp.z = flip(wp.z,(IN.vertexWorldPos.xyz+_Offset).z);

			float2 uvX = float2(flip(wp.z,vn.x), wp.y);
			float2 uvY = float2(1 - wp.x, flip(wp.z,-vn.y));
			float2 uvZ = float2(1 - wp.x, flip(wp.y,vn.z));


			float4 finalTex = texTriplanarUVBased(_MainTex,uvX,uvY,uvZ,vn);
			float3 finalNorm = UnpackNormal(texTriplanarUVBased(_BumpMap,uvX,uvY,uvZ,vn));
			float3 pbr = clamp(texTriplanarUVBased(_PBR,uvX,uvY,uvZ,vn),0,1);
			float3 emissive = clamp(texTriplanarUVBased(_Emission,uvX,uvY,uvZ,vn),0,1);


			o.Albedo = finalTex.rgb * _Color;
			//o.Albedo = float3(uvX,0);
			//o.Albedo = wp;
			//o.Albedo = clamp(pbr.rgb,0,1);

			o.Normal = finalNorm * _NormalStrength;


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