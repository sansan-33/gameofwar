Shader "AppsTools/FastShader/Effect/Distortion"
{
	Properties {
		_NormalMap("Normal Map", 2D) = "bump" {}
		_Distortionpower("Distortion power", Float) = 0.05
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	}

	SubShader {

		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		Lighting Off 
		ZWrite Off
		Pass {
			Tags { "LightMode" = "UniversalForward" }
			
			HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest

				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

				struct appdata_t {
					float4 vertex : POSITION;
					float4 color : COLOR;
					float2 texcoord: TEXCOORD0;
				};

				struct v2f {
					float4 vertex : POSITION;
					float4 uvgrab : TEXCOORD0;
					float4 texcoord : TEXCOORD1;
                    float4 screenPos : TEXCOORD2;
                    float4 color : COLOR;
					float2 texcoord2 : TEXCOORD3;
					float4 projPos : TEXCOORD4;
				};

				#pragma target 3.0
           		uniform float _InvFade;
				uniform sampler2D _GrabTexture;
				uniform sampler2D _NormalMap;
				uniform float4 _NormalMap_ST;
				uniform float _Distortionpower;	
				uniform float4 _GrabTexture_TexelSize;	
				SAMPLER(_CameraOpaqueTexture);

				v2f vert (appdata_t v)
				{
					v2f o;
					VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
					o.vertex = vertexInput.positionCS;

					//o.texcoord = TRANSFORM_TEX( v.texcoord, _Albedo );
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

				half4 frag( v2f i ) : SV_Target
				{
					half3 tex2DNode14 = UnpackNormal(tex2D( _NormalMap, i.texcoord2));
					half2 screenColor29 = tex2DNode14.rg;
					half clampResult89 = (abs(tex2DNode14.r) + abs(tex2DNode14.g) * 30) - 0.03;
					screenColor29 = screenColor29 * _GrabTexture_TexelSize.xy * _Distortionpower * i.color.a;
					i.texcoord.xy = screenColor29 * i.texcoord.z + i.texcoord.xy;
					half4 col = tex2Dproj( _CameraOpaqueTexture, i.texcoord);
					col.a = saturate(col.a * clampResult89);
                    return col;
				}
			ENDHLSL
		}
	}
}