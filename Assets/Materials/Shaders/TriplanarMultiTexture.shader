Shader "LMAO2/TriplanarMultiTexture" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
        _MainTexX ("Tex X", 2D) = "white" {}
		_MainTexY ("Tex Y", 2D) = "white" {}
		_MainTexZ ("Tex Z", 2D) = "white" {}
        _BumpMapX ("Bumpmap X", 2D) = "bump" {}
		_BumpMapY ("Bumpmap Y", 2D) = "bump" {}
		_BumpMapZ ("Bumpmap Z", 2D) = "bump" {}
		_PBRX ("PBR X", 2D) = "bump" {} //Red: Smoothness, Green: Metallic, Blue: Height
		_PBRY ("PBR Y", 2D) = "bump" {}
		_PBRZ ("PBR Z", 2D) = "bump" {}

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_BorderFade ("BorderFade", Range(0,100)) = 10
		_TexScale ("Texture scaling", Vector) = (1,1,1,1)
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 3.0
		
		float PI = 3.14159265359;


		struct Input {
			float3 vertexNormal;
			float3 worldPos;
		};


		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.vertexNormal = mul(unity_ObjectToWorld,v.normal);
		}


		sampler2D _MainTexX;
		sampler2D _MainTexY;
		sampler2D _MainTexZ;
		sampler2D _BumpMapX;
		sampler2D _BumpMapY;
		sampler2D _BumpMapZ;
		sampler2D _PBRX;
		sampler2D _PBRY;
		sampler2D _PBRZ;

		half _Glossiness;
        half _Metallic;
		half _BorderFade;
        fixed4 _Color;
		float4 _TexScale;


		void surf (Input IN, inout SurfaceOutputStandard o) {
			float4 texX = tex2D (_MainTexX, IN.worldPos.yz*_TexScale.x);
			float4 texY = tex2D (_MainTexY, IN.worldPos.xz*_TexScale.y);
			float4 texZ = tex2D (_MainTexZ, IN.worldPos.xy*_TexScale.z);

			float3 normX = UnpackNormal (tex2D (_BumpMapX, IN.worldPos.yz*_TexScale.x));
			float3 normY = UnpackNormal (tex2D (_BumpMapY, IN.worldPos.xz*_TexScale.y));
			float3 normZ = UnpackNormal (tex2D (_BumpMapZ, IN.worldPos.xy*_TexScale.z));

			float3 pbrX = tex2D (_PBRX, IN.worldPos.yz*_TexScale.x).rgb;
			float3 pbrY = tex2D (_PBRY, IN.worldPos.xz*_TexScale.y).rgb;
			float3 pbrZ = tex2D (_PBRZ, IN.worldPos.xy*_TexScale.z).rgb;

			float3 face = pow(abs(IN.vertexNormal.rgb),_BorderFade);
			face = face/(face.x+face.y+face.z);

			float4 finalTex = texX*face.x+texY*face.y+texZ*face.z;
			float3 finalNorm = normX*face.x+normY*face.y+normZ*face.z;
			float3 pbr = pbrX*face.x+pbrY*face.y+pbrZ*face.z;

			o.Albedo = finalTex.rgb;
			//o.Albedo *= face;

			o.Normal = finalNorm;

            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic * pbr.g;
            o.Smoothness = _Glossiness * pbr.r;
            o.Alpha = finalTex.a;
		}


		ENDCG
	} 
	Fallback "Diffuse"
}