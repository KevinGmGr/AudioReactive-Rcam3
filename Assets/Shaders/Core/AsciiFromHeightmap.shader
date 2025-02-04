Shader "Custom/AsciiFromHeightmap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            int character(int n, float2 p)
            {
                p = floor(p * float2(-4.0, 4.0) + 2.5);
                if (p.x >= 0.0 && p.x <= 4.0 && p.y >= 0.0 && p.y <= 4.0)
                {
                    int a = int(round(p.x) + 5.0 * round(p.y));
                    if (((n >> a) & 1) == 1) return 1;
                }
                return 0;
            }

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

            fixed4 frag (v2f i) : SV_Target
            {
                float2 pix = i.uv * _ScreenParams.xy;
                float3 col = tex2D(_MainTex, floor(pix / 8.0) * 8.0 / _ScreenParams.xy).rgb;

                float gray = 0.3 * col.r + 0.59 * col.g + 0.11 * col.b;
                int n = 4096;

                if (gray > 0.2) n = 65600;  
                if (gray > 0.3) n = 163153;  
                if (gray > 0.4) n = 15255086;  
                if (gray > 0.5) n = 13121101;  
                if (gray > 0.6) n = 15252014;  
                if (gray > 0.7) n = 13195790;  
                if (gray > 0.8) n = 11512810;  

                float2 p = fmod(pix / 4.0, 2.0) - float2(1.0, 1.0);
                float asciiPixel = character(n, p);

                col = lerp(col, float3(asciiPixel, asciiPixel, asciiPixel), asciiPixel);

                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}
