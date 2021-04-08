Shader "Hidden/SSAOCombineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_AOIntensity("AOIntensity", Float) = 1
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
			sampler2D _SSAO;

			float _AOIntensity = 1.0;

			float4 _AOColor = float4(0.0, 0.0, 0.0, 0.0);

            fixed4 frag (v2f i) : SV_Target
            {
				half4 c = tex2D(_MainTex, i.uv); 
				half ao = tex2D(_SSAO, i.uv).r;

				float aoPow = pow(ao, _AOIntensity);
				half3 aoColored = _AOColor.rgb * (1.0 - ao);

				c.rgb *= aoPow;
				c.rgb += aoColored;
				return c;
            }
            ENDCG
        }
    }
}
