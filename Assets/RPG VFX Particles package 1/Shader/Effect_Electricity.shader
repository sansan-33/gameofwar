Shader "AppsTools/FastShader/Effect/Electricity"
{
	Properties
	{
		_MainTexture("Main Texture", 2D) = "white" {}
		_Dissolveamount("Dissolve amount", Range( 0 , 1)) = 0.332
		_Mask("Mask", 2D) = "white" {}
		_Color("Color", Color) = (0.5,0.5,0.5,1)
		_Emission("Emission", Float) = 6
		_RemapXYFresnelZW("Remap XY/Fresnel ZW", Vector) = (-10,10,2,2)
		_Speed("Speed", Vector) = (0.189,0.225,-0.2,-0.05)
		_Opacity("Opacity", Range( 0 , 1)) = 1
		[MaterialToggle] _Usedepth ("Use depth?", Float ) = 0
        _Depth ("Depth", Float ) = 0.15
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
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					float3 ase_normal : NORMAL;
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
					float4 ase_texcoord3 : TEXCOORD3;
					float4 ase_texcoord4 : TEXCOORD4;
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif


				uniform sampler2D _Mask;
				uniform float _Dissolveamount;
				uniform sampler2D _MainTexture;
				uniform float4 _Speed;
				uniform float4 _MainTexture_ST;
				uniform float4 _RemapXYFresnelZW;
				uniform float4 _Color;
				uniform float _Emission;
				uniform float _Opacity;
				uniform float _Depth;
				uniform fixed _Usedepth;

				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.ase_texcoord3.xyz = ase_worldPos;
					float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
					o.ase_texcoord4.xyz = ase_worldNormal;
					
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord3.w = 0;
					o.ase_texcoord4.w = 0;

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
					

					float temp_output_66_0 = (-0.65 + ((1.0 + (_Dissolveamount - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) - 0.0) * (0.65 - -0.65) / (1.0 - 0.0));
					float2 appendResult21 = (float2(_Speed.x , _Speed.y));
					float2 uv0_MainTexture = i.texcoord.xy * _MainTexture_ST.xy + _MainTexture_ST.zw;
					float2 appendResult22 = (float2(_Speed.z , _Speed.w));
					float2 appendResult74 = (float2((1.0 + (saturate( (_RemapXYFresnelZW.x + (( ( temp_output_66_0 + tex2D( _MainTexture, ( ( appendResult21 * _Time.y ) + uv0_MainTexture ) ).r ) * ( temp_output_66_0 + tex2D( _MainTexture, ( uv0_MainTexture + ( _Time.y * appendResult22 ) ) ).r ) ) - 0.0) * (_RemapXYFresnelZW.y - _RemapXYFresnelZW.x) / (1.0 - 0.0)) ) - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) , 0.0));
					float temp_output_120_0 = saturate( tex2D( _Mask, appendResult74 ).r );
					float3 ase_worldPos = i.ase_texcoord3.xyz;
					float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
					ase_worldViewDir = normalize(ase_worldViewDir);
					float3 ase_worldNormal = i.ase_texcoord4.xyz;
					float fresnelNdotV83 = dot( ase_worldNormal, ase_worldViewDir );
					float fresnelNode83 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV83, _RemapXYFresnelZW.z ) );
					float clampResult78 = clamp( ( _RemapXYFresnelZW.w * fresnelNode83 ) , 0.0 , 1.0 );
					float4 appendResult116 = (float4((( temp_output_120_0 * _Color * i.color * clampResult78 * _Emission )).rgb , ( temp_output_120_0 * _Color.a * i.color.a * clampResult78 * _Opacity )));
					

					fixed4 col = appendResult116;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	
	
	
}