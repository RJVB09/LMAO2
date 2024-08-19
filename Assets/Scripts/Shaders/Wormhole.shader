Shader "ImageEffect/Wormhole"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DistortionGlobal ("DistortionGlobal", float) = 0
        [ShowAsVector2] _ScreenSize("ScreenSize", Vector) = (0, 0, 0, 0)
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

            sampler2D _MainTex;
            float _DistortionGlobal;
            float4 _ScreenSize;
            float4 _Tint;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 screenCenter = float2(_ScreenSize.x/2.0,_ScreenSize.y/2.0);
                float2 dir = normalize((i.uv * screenCenter * 2) - screenCenter);
                float2 distortion = -normalize((i.uv * screenCenter * 2) - screenCenter); //weee wooo 

                float4 col = tex2D(_MainTex, i.uv + 0.07 * _DistortionGlobal * distortion) * _Tint;
                //float4 col = distortionLinesMask;
                return col;
            }
            ENDCG
        }
    }
}
