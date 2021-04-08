Shader "Hidden/BasicMobileSSAO"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_RandomTex("Random tex", 2D) = "white" {} 
    
		[Range(0, 1)]
		_SelfShadowingReduction("Self Shadowing Reduction", Range(0, 1)) = 0.2 
		_Area("Acceptable depth difference", Range(0, 1.5)) = 0.75
		_OcclusionFactor("OcclusionFactor", Range(0, 20)) = 0.1
		_Radius("Radius", Range(0.01, 0.3)) = 0.05

		_DepthCutoff("Depth cutoff", Range(0, 1)) = 0.8
		_DepthCutoffSmoothTransitionRange("Depth cutoff smooth transition range", Range(0, 0.1)) = 0.05

		_SamplesCount("Samples count", Range(1, 64)) = 8
			  
		[Toggle(COMPLEX_NORMALS)]
		_ComplexNormals("Use complex normals calculation", Float) = 0
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
			 
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma shader_feature COMPLEX_NORMALS

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
				float2 cameraDepthUV : TEXCOORD1;
				float2 randomTexUV : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

			float4 _CameraDepthNormalsTexture_ST, _CameraDepthTexture_ST, _RandomTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

				o.cameraDepthUV = TRANSFORM_TEX(v.uv, _CameraDepthTexture);
				o.randomTexUV = TRANSFORM_TEX(v.uv, _RandomTex);

                return o;
            }

			sampler2D _CameraDepthTexture, _RandomTex, _MainTex;
			float4 _CameraDepthTexture_TexelSize, _RandomTex_TexelSize;

			float2 _ScreenSize; //current render texture resolution
			 
			float _Base;
			float _Area;
			float _OcclusionFactor;
			float _Radius;
			float _SamplesCount;
			float _SelfShadowingReduction;
			float _DepthCutoff, _DepthCutoffSmoothTransitionRange; 
			  
			#define KERNELS_MAX_AMOUNT 64 
			 
			float getDepthFromDepthTexture(float2 uv) {
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
				depth = Linear01Depth(depth); //0 = near plane, 1 = far plane   
				depth *= _ProjectionParams.z;
				return depth;
			}

			float3 getNormalFromDepthTextureSimple(float depth, float2 texcoords) {
				float2 offsety = float2(0.0, _CameraDepthTexture_TexelSize.y);
				float2 offsetx = float2(_CameraDepthTexture_TexelSize.x, 0.0); //_CameraDepthTexture_TexelSize.x == 1 pixel on x axis
				 
				float depthu = getDepthFromDepthTexture(texcoords + offsety);
				float depthr = getDepthFromDepthTexture(texcoords + offsetx);

				float3 p1 = float3(offsety, depthu - depth);
				float3 p2 = float3(offsetx, depthr - depth);
				float3 normal = cross(p1, p2);

				normal.z = -normal.z; 
				return normalize(normal);
			}
			 
			float3 getNormalFromDepthTextureComplex(float depth, float2 texcoords)
			{
				float2 offsety = float2(0.0, _CameraDepthTexture_TexelSize.y);
				float2 offsetx = float2(_CameraDepthTexture_TexelSize.x, 0.0); //_CameraDepthTexture_TexelSize.x == 1 pixel on x axis

				//4 points and center point for cross pattern
				float depthu = getDepthFromDepthTexture(texcoords + offsety);
				float depthr = getDepthFromDepthTexture(texcoords + offsetx);
				float depthd = getDepthFromDepthTexture(texcoords - offsety);
				float depthl = getDepthFromDepthTexture(texcoords - offsetx);
				 
				int testx = abs(depthr - depth) > abs(depthl - depth);
				int testy = abs(depthu - depth) > abs(depthd - depth);
				 
				float3 p1 = float3(0, 0, 0);
				float3 p2 = float3(0, 0, 0);
				float3 normal = float3(0, 1, 0);
				
				//make triangle from depth, depthr, depthu
				if (testx == 0 && testy == 0) {
					p1 = float3(offsety, depthu - depth);
					p2 = float3(offsetx, depthr - depth);
					normal = cross(p1, p2);
				}
				
				//depth, depthl, depthu
				else if (testx == 1 && testy == 0) {
					p1 = float3(offsety, depthu - depth);
					p2 = float3(-offsetx, depthl - depth);
					normal = cross(p2, p1);
				}
				
				//depth, depthl, depthd
				else if (testx == 1 && testy == 1) {
					p1 = float3(-offsety, depthd - depth);
					p2 = float3(-offsetx, depthl - depth);
					normal = cross(p1, p2);
				}

				//depth, depthr, depthd
				else if (testx == 0 && testy == 1) {
					p1 = float3(-offsety, depthd - depth);
					p2 = float3(offsetx, depthr - depth);
					normal = cross(p2, p1);
				}

				normal.z = -normal.z;

				return normalize(normal); 
			}
			 
			fixed4 frag(v2f i) : SV_Target
			{
				//declare random kernels
				static float3 kernels[KERNELS_MAX_AMOUNT] = {
					 float3(0.06328941, 0.01994319, 0.07481124),
					 float3(0.03700708, 0.00807255, 0.09278633),
					 float3(-0.07120832, 0.06711908, 0.02451449),
					 float3(-0.07013409, -0.02743604, 0.06875968),
					 float3(0.08343524, 0.02775943, 0.05462104),
					 float3(0.07777761, -0.02806062, 0.06551376),
					 float3(-0.06979697, 0.04488504, 0.06898056),
					 float3(0.06152402, -0.0775122, 0.04975837),
					 float3(0.09037338, 0.02177688, 0.06609595),
					 float3(-0.06884602, 0.0739684, 0.06054119),
					 float3(0.06138743, 0.0869082, 0.05963118),
					 float3(-0.06846327, -0.09353597, 0.05087288),
					 float3(0.09869569, -0.05618825, 0.06656799),
					 float3(0.01049563, 0.1243686, 0.05681524),
					 float3(-0.06136484, 0.1289595, 0.00847303),
					 float3(-0.04512313, -0.1283092, 0.0619073),
					 float3(0.1279057, 0.08463797, 0.02984291),
					 float3(0.1294072, 0.08463719, 0.0531309),
					 float3(-0.1487408, -0.07062645, 0.04684645),
					 float3(-0.1111765, 0.1406965, 0.0006488283),
					 float3(-0.07517008, 0.1277841, 0.1154277),
					 float3(0.09164333, 0.06959662, 0.1597723),
					 float3(-0.1964463, -0.04821623, 0.04078457),
					 float3(0.1942053, -0.04459439, 0.08398435),
					 float3(-0.1894196, 0.06360708, 0.1067939),
					 float3(0.1244859, -0.1960531, 0.0489032),
					 float3(0.1530032, -0.1187635, 0.1557401),
					 float3(-0.04495911, 0.1079154, 0.2324369),
					 float3(-0.1490425, 0.1766552, 0.1439023),
					 float3(0.153011, 0.2401935, 0.0002976091),
					 float3(0.0318984, -0.2859344, 0.07669028),
					 float3(-0.2032115, 0.01141958, 0.2353582),
					 float3(0.1945063, -0.1762801, 0.1916184),
					 float3(-0.2273341, -0.1781176, 0.178061),
					 float3(-0.1286226, -0.2582512, 0.2051373),
					 float3(-0.2371047, -0.2751156, 0.06614832),
					 float3(0.2052595, 0.2384454, 0.221488),
					 float3(0.2808193, -0.2399395, 0.1556108),
					 float3(0.1731628, 0.314054, 0.2133345),
					 float3(-0.3733295, 0.06918222, 0.2106469),
					 float3(-0.3433378, -0.1496076, 0.2522804),
					 float3(0.4202956, -0.1177935, 0.1725555),
					 float3(-0.101023, 0.184311, 0.4399719),
					 float3(-0.3949001, -0.2477703, 0.1974274),
					 float3(-0.3521414, -0.006751961, 0.389854),
					 float3(0.02223781, -0.5175605, 0.1691246),
					 float3(-0.1632522, -0.4622954, 0.2806964),
					 float3(-0.0009258965, -0.5508124, 0.1981662),
					 float3(0.3988319, 0.07460167, 0.4504517),
					 float3(0.4921954, 0.05010534, 0.3860946),
					 float3(0.5102895, 0.06211282, 0.3966843),
					 float3(0.4797106, -0.3975273, 0.250547),
					 float3(-0.3730522, 0.4261965, 0.4012727),
					 float3(-0.3388773, 0.5928005, 0.2194144),
					 float3(0.5145497, -0.4460288, 0.2914908),
					 float3(-0.6630164, -0.3427285, 0.166346),
					 float3(-0.6306157, -0.3603634, 0.3083532),
					 float3(0.5171435, -0.6203834, 0.1005308),
					 float3(0.5490472, -0.4447456, 0.452701),
					 float3(-0.8384638, 0.1831153, 0.1069774),
					 float3(0.5064176, 0.4888669, 0.5463142),
					 float3(0.2203732, 0.6102141, 0.6488981),
					 float3(0.9068054, -0.2497792, 0.08739639),
					 float3(0.6749795, -0.5211893, 0.4666181)
				};
				
				//read random normal from noise texture 
				half2 NoiseScale = half2(_ScreenSize.x / _RandomTex_TexelSize.z, _ScreenSize.y / _RandomTex_TexelSize.w);
				half3 randomNormal = UnpackNormal(tex2D(_RandomTex, i.randomTexUV * NoiseScale));

				//read scene depth, normal
				float3 viewNormal; 
				float depth = getDepthFromDepthTexture(i.cameraDepthUV);
				
				#ifdef COMPLEX_NORMALS
				viewNormal = getNormalFromDepthTextureComplex(depth, i.cameraDepthUV); 
				#else 
				viewNormal = getNormalFromDepthTextureSimple(depth, i.cameraDepthUV);
				#endif

				float scale = _Radius / depth; 
				float depth01 = depth / _ProjectionParams.z; 

				//accumulated occlusion factor
				float occ = 0.0;  
				for (int s = 0; s < _SamplesCount; s++)
				{
					//reflect sample direction around a random vector
					half3 randomDir = reflect(kernels[s], randomNormal);

					//make it point to the upper hemisphere
					half flip = sign(dot(viewNormal, randomDir));  
					randomDir *= flip;

					//reduce self shadowing
					randomDir += viewNormal * _SelfShadowingReduction;

					float2 offset = randomDir.xy * scale;
					float sD = depth - (randomDir.z * _Radius);

					//get depth at offset position 
					float currentDepth = getDepthFromDepthTexture(i.cameraDepthUV + offset);  
					float depthDifference = saturate(sD - currentDepth);

					//occ += step(_Falloff, depthDifference) * (1.0 - smoothstep(_Falloff, _Area, depthDifference)); 
					if (depthDifference < _Area) {
						occ += pow(depthDifference, _OcclusionFactor);
					}
				}
				float ao = 1.0 - occ * (1.0 / _SamplesCount); 

				//depth smooth cutoff at far plane bounds
				fixed toFarAway = smoothstep(_DepthCutoff - _DepthCutoffSmoothTransitionRange, _DepthCutoff, depth01);
				fixed final = lerp(ao, 1.0, toFarAway);
				  
				return fixed4(final, final, final, 1.0);
				//return float4(viewNormal, 1.0);
			}
            ENDCG
        } 
    }
}
