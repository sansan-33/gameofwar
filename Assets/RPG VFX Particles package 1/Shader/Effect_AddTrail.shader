Shader "AppsTools/FastShader/Effect/AddTrail"
{
	Properties
	{
		_MainTexture("MainTexture", 2D) = "white" {}
		_SpeedMainTexUVNoiseZW("Speed MainTex U/V + Noise Z/W", Vector) = (0,0,0,0)
		_StartColor("StartColor", Color) = (1,0,0,1)
		_EndColor("EndColor", Color) = (1,1,0,1)
		_Colorpower("Color power", Float) = 1
		_Colorrange("Color range", Float) = 1
		_Noise("Noise", 2D) = "white" {}
		_Emission("Emission", Float) = 2
		[Toggle]_Usedark("Use dark", Float) = 1
		[Toggle]_Mask("Mask", Float) = 0
		_Maskpower("Mask power", Float) = 10
		[MaterialToggle] _Usedepth ("Use depth?", Float ) = 0
		_Depthpower("Depth power", Float) = 1
		[Enum(Cull Off,0, Cull Front,1, Cull Back,2)] _CullMode("Culling", Float) = 2
	}

	Category 
	{
		SubShader
		{
			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull[_CullMode]
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM	
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityShaderVariables.cginc"
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

			

				uniform float4 _StartColor;
				uniform float4 _EndColor;
				uniform float _Colorrange;
				uniform float _Colorpower;
				uniform float _Emission;
				uniform float _Usedark;
				uniform sampler2D _MainTexture;
				uniform float4 _SpeedMainTexUVNoiseZW;
				uniform float4 _MainTexture_ST;
				uniform sampler2D _Noise;
				uniform float4 _Noise_ST;
				uniform float _Mask;
				uniform float _Maskpower;
				uniform fixed _Usedepth;
				uniform float _Depthpower;

				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					float lp = 1;
				
					float2 uv01 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float U6 = uv01.x;
					float4 lerpResult3 = lerp( _StartColor , _EndColor , saturate( pow( ( U6 * _Colorrange ) , _Colorpower ) ));
					float4 temp_cast_0 = (1.0).xxxx;
					float2 appendResult32 = (float2(_SpeedMainTexUVNoiseZW.x , _SpeedMainTexUVNoiseZW.y));
					float3 uv0_MainTexture = i.texcoord.xyz;
					uv0_MainTexture.xy = i.texcoord.xyz.xy * _MainTexture_ST.xy + _MainTexture_ST.zw;
					float4 Main57 = tex2D( _MainTexture, ( float3( ( appendResult32 * _Time.y ) ,  0.0 ) + uv0_MainTexture + uv0_MainTexture.z ).xy );
					float2 uv0_Noise = i.texcoord.xy * _Noise_ST.xy + _Noise_ST.zw;
					float2 appendResult29 = (float2(_SpeedMainTexUVNoiseZW.z , _SpeedMainTexUVNoiseZW.w));
					float clampResult44 = clamp( ( pow( ( 1.0 - U6 ) , 0.8 ) * 1.0 ) , 0.2 , 0.6 );
					float4 temp_cast_3 = (U6).xxxx;
					float4 Dissolve49 = saturate( ( ( tex2D( _Noise, ( uv0_Noise + ( _Time.y * appendResult29 ) ) ) + clampResult44 ) - temp_cast_3 ) );
					float V17 = uv01.y;
					float4 temp_output_51_0 = ( i.color.a * Main57 * Dissolve49 * saturate( ( (1.0 + (U6 - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) * (1.0 + (V17 - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) * V17 * 6.0 * lerp(1.0,( U6 * _Maskpower ),_Mask) ) ) );
					float4 appendResult92 = (float4((( ( lerpResult3 * i.color * _Emission ) * lerp(temp_cast_0,temp_output_51_0,_Usedark) )).rgb , temp_output_51_0.r));
					
					fixed4 col = appendResult92;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}	
}