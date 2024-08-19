Shader "LMAO2/Screen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent"}
        LOD 100

        GrabPass { }

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
                float4 screenSpace : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _GrabTexture;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenSpace = ComputeScreenPos(o.vertex);
                return o;
            }

            float3 Screen(float3 a, float3 b)
            {
                return 1 - (1-a)*(1-b);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 screenUV = i.screenSpace.xy / i.screenSpace.w;

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv)*_Color;
                fixed4 bg = tex2D(_GrabTexture, screenUV);

                col = float4(Screen(col.rgb,bg.rgb),1);

                return col;
            }
            ENDCG
        }
    }
}
