Shader "AppsTools/FastShader/Effect/Effect_Warp" {
    Properties {
		_Albedo ("_Albedo(_Albedo)", 2D) = "white" {}
        _Alpha ("_Alpha", Float ) = 0
        _warp ("_warp", Range(0, 1)) = 0
	}

	SubShader {
		  Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent-1"
            "RenderType"="Transparent"
        }
		Blend SrcAlpha OneMinusSrcAlpha
		//AlphaTest Greater .01
		Cull Off Lighting Off ZWrite Off
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
					float4 vertexColor : COLOR;
					float2 texcoord0: TEXCOORD0;
				};

				struct v2f {
					float4 pos : POSITION;
					float4 uvgrab : TEXCOORD0;
					float2 uv0 : TEXCOORD1;
                    float4 screenPos : TEXCOORD2;
                    float4 vertexColor : COLOR;
				};

          
            uniform sampler2D _GrabTexture;
            uniform sampler2D _Albedo; uniform float4 _Albedo_ST;
            uniform float _Alpha;
            uniform float _warp;
				SAMPLER(_CameraOpaqueTexture);

				v2f vert (appdata_t v)
				{
					v2f o;
					VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
					o.pos = vertexInput.positionCS;

					#if UNITY_UV_STARTS_AT_TOP
					float scale = -1.0;
					#else
					float scale = 1.0;
					#endif

				
					o.uv0 = TRANSFORM_TEX( v.texcoord0, _Albedo );
                    o.vertexColor = v.vertexColor;

                    o.screenPos = o.pos;


					//o.uvmain = TRANSFORM_TEX( v.texcoord, _NoiseTex);
					return o;
				}

				half4 frag( v2f i ) : SV_Target
				{
					#if UNITY_UV_STARTS_AT_TOP
                    float grabSign = -_ProjectionParams.x;
                    #else
                        float grabSign = _ProjectionParams.x;
                    #endif
                    i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
                    i.screenPos.y *= _ProjectionParams.x;
                    float4 _Albedo_var = tex2D(_Albedo,TRANSFORM_TEX(i.uv0, _Albedo));
                    float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5 + (float2(_Albedo_var.r,_Albedo_var.g)*_Albedo_var.a*_warp*i.vertexColor.a);

                     return float4(lerp(tex2D(_CameraOpaqueTexture, sceneUVs).rgb, 0,_Alpha),1);
				}
			ENDHLSL
		}
	}
}
