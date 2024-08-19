Shader "LMAO2/StandardTransparent" {
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
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }

        // Opaque pass with ZWrite On
        Pass {
            ZWrite On
            ColorMask 0 // Don't write color, only depth

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct v2f {
                float4 pos : SV_POSITION;
            };

            struct appdata_full {
                float4 vertex : POSITION;
            };

            v2f vert(appdata_full v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                return float4(0, 0, 0, 0); // Doesn't matter, no color written
            }
            ENDCG
        }

        // Main transparent pass
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert alpha:fade
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
            o.Normal = finalNorm;
            o.Emission = emissive * _EmissionColor;

            o.Metallic = _Metallic * pbr.g;
            o.Smoothness = _Glossiness * pbr.r;
            o.Alpha = finalTex.a * _Color.a;
        }

        ENDCG
    }
    Fallback "Diffuse"
}
