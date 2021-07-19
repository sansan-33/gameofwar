Shader "AppsTools/FastShader/Effect/SmoothSmoke"
{
	Properties
	{	
		_MainTex("MainTex", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Emission("Emission", Float) = 2
		[Toggle]_Useblack("Use black", Float) = 0
		[MaterialToggle] _Usedepth ("Use depth?", Float ) = 0
		_Depthpower("Depth power", Float) = 1
	}


	Category 
	{
		SubShader
		{
			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {		
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma target 2.0
				#pragma multi_compile_particles
				#pragma multi_compile_fog			
				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID			
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO			
				};		
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif


				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				uniform float4 _Color;
				uniform float _Useblack;
				uniform float _Emission;
				uniform float _Depthpower;
				uniform fixed _Usedepth;

				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					float lp = 1;
					

					float4 uv0_MainTex = i.texcoord;
					uv0_MainTex.xy = i.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
					float smoothstepResult1 = smoothstep( uv0_MainTex.z , ( uv0_MainTex.z + uv0_MainTex.w ) , tex2D( _MainTex, uv0_MainTex.xy ).a);
					float clampResult11 = clamp( smoothstepResult1 , 0.0 , 1.0 );
					float4 appendResult27 = (float4((( _Color * lerp(1.0,clampResult11,_Useblack) * _Emission * i.color )).rgb , ( _Color.a * clampResult11 * i.color.a )));
					
					fixed4 col = appendResult27;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
}