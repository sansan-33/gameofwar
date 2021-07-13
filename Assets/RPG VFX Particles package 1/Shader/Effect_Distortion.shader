Shader "AppsTools/FastShader/Effect/Distortion"
{
	Properties
	{
		_NormalMap("Normal Map", 2D) = "bump" {}
		_Distortionpower("Distortion power", Float) = 0.05
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	}

	Category 
	{
		SubShader
		{
			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			Lighting Off 
			ZWrite Off
			Fog { Mode Off}
			GrabPass{ }

			Pass {		
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_particles
				#include "UnityCG.cginc"
				uniform sampler2D_float _CameraDepthTexture;
				uniform float _InvFade;
				uniform sampler2D _GrabTexture;
				uniform sampler2D _NormalMap;
				uniform float4 _NormalMap_ST;
				uniform float _Distortionpower;	
				uniform float4 _GrabTexture_TexelSize;	

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;				
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD1;
					float2 texcoord2 : TEXCOORD2;
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD3;
					#endif
				};			

				v2f vert ( appdata_t v  )
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
				
					o.color = v.color;
					
					#if UNITY_UV_STARTS_AT_TOP
					half scale = -1.0;
					#else
					half scale = 1.0;
					#endif
					o.texcoord.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
					o.texcoord.zw = o.vertex.w;					
					#if UNITY_SINGLE_PASS_STEREO
					o.texcoord.xy = TransformStereoScreenSpaceTex(o.texcoord.xy, o.texcoord.w);
					#endif
					o.texcoord.z /= distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, v.vertex));
					o.texcoord2 = TRANSFORM_TEX( v.texcoord, _NormalMap );
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					

					half3 tex2DNode14 = UnpackNormal(tex2D( _NormalMap, i.texcoord2));
					half2 screenColor29 = tex2DNode14.rg;
					half clampResult89 = (abs(tex2DNode14.r) + abs(tex2DNode14.g) * 30) - 0.03;
					screenColor29 = screenColor29 * _GrabTexture_TexelSize.xy * _Distortionpower * i.color.a;
					i.texcoord.xy = screenColor29 * i.texcoord.z + i.texcoord.xy;
					half4 col = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(i.texcoord));
					col.a = saturate(col.a * clampResult89);
					return col;
				}
				ENDCG 
			}
		}	
	}	
}