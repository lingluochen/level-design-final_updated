Shader "Hidden/SSAOBlurShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
			float4 _MainTex_ST;
			 
			sampler2D _CameraDepthTexture;

			float2 _Dir;
			float BlurRadius;
			float BlurDepthFalloff;
			float _CameraFarPlane;
			 
			float getDepthFromDepthTexture(float2 uv) {
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
				depth = Linear01Depth(depth);   
				return depth;
			}

            fixed4 frag (v2f input) : SV_Target
            {
				float weights[4] = { 0.0702702703, 0.3162162162, 0.3162162162, 0.0702702703 };
				float offsets[4] = { -3.2307692308, -1.3846153846, 1.3846153846, 3.2307692308 };
				 
				float2 dir = _Dir * _MainTex_ST.xy * BlurRadius;
				 
				float depth = getDepthFromDepthTexture(input.uv);
				float3 centerColor = tex2D(_MainTex, input.uv).xyz * 2.0 - 1.0;

				float3 result = centerColor * 0.2270270270; 
				float weightsSum = 0.2270270270;
				 
				for (int c = 0; c < 4; c++) {
					if(abs(depth - getDepthFromDepthTexture(input.uv + offsets[c] * dir)) < BlurDepthFalloff) {
						result += (tex2D(_MainTex, input.uv + offsets[c] * dir).xyz * 2.0 - 1.0) * weights[c];
						weightsSum += weights[c];
					}
				}

				result *= 1.0 / weightsSum;

				return float4(result * 0.5 + 0.5, 1.0);
            }
            ENDCG
        }
    }
}
