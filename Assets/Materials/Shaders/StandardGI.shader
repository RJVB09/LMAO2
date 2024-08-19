Shader "LMAO2/StandardGI" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Tex", 2D) = "white" {}
        _BumpMap ("Bumpmap", 2D) = "bump" {}
		_PBR ("PBR", 2D) = "white" {} //Red: Smoothness, Green: Metallic, Blue: Height
		_EmissionTex ("Emission", 2D) = "black" {}

		[HDR]_EmissionColor ("Emission Color", Color) = (1,1,1,1)

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		// Meta Pass for Global Illumination
        Pass {
            Name "META"
            Tags {"LightMode"="Meta"}
            Cull Off
            CGPROGRAM
            #include "UnityStandardMeta.cginc"

            sampler2D _GIAlbedoTex;
            fixed4 _GIAlbedoColor;
            float4 frag_meta2 (v2f_meta i): SV_Target {
                FragmentCommonData data = UNITY_SETUP_BRDF_INPUT(i.uv);
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT(UnityMetaInput, o);
                fixed4 c = tex2D(_MainTex, i.uv);
                //o.Albedo = fixed3(1,1,1);
                //o.Emission = Emission(i.uv.xy) * _Emission.rgb;
                return UnityMetaFragment(o);
            }

            #pragma vertex vert_meta
            #pragma fragment frag_meta2
            ENDCG
        }


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
		sampler2D _EmissionTex;

		half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		float4 _EmissionColor;

		#include "UnityCG.cginc"

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float4 finalTex = tex2D(_MainTex,IN.uv_MainTex);
			float3 finalNorm = UnpackNormal(tex2D(_BumpMap,IN.uv_MainTex));
			float3 pbr = clamp(tex2D(_PBR,IN.uv_MainTex),0,1);
			float3 emissive = clamp(tex2D(_EmissionTex,IN.uv_MainTex),0,1);


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