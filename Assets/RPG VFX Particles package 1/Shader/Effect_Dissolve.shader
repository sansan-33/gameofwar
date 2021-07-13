Shader "AppsTools/FastShader/Effect/Dissolve_Add"
{
	Properties
	{
		_MainTex("Main", 2D) = "white" {}
		_DissolveTex("RongJie", 2D) = "white" {}
		_DissSize("DissSize", Range(0, 1)) = 0.1
		_DissColor("DissColor", Color) = (1,0,0,1)
		_AddColor("AddColor", Color) = (1,1,0,1)
		_Value("Value", Range(0,1)) = 0.5 
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 1  
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 1  
		[Enum(Off, 0, On, 1)] _ZWrite("ZWrite", Float) = 0  
		//[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0  */
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 1
		[KeywordEnum(YES, NO)] CUSTOMDATA_OPEN("Whether to use the particle system CustomData Custom1.x to control the dissolution", Float) = 0
	}
		SubShader
		{
			Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
			LOD 100
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
			//ZTest[_ZTest]
			//Blend SrcAlpha OneMinusSrcAlpha
			//Blend SrcAlpha One 

			ColorMask RGB
			Lighting Off ZWrite Off

			Cull[_Cull] 
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma shader_feature CUSTOMDATA_OPEN_YES
				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float4 uv : TEXCOORD0;
					fixed4 texcoord1 : TEXCOORD1;
					fixed4 vertexColor : COLOR;
				};

				struct v2f
				{
					float4 uv : TEXCOORD0;
					fixed4 uv1 : TEXCOORD1;
					float4 vertex : SV_POSITION;
					fixed4 vertexColor : COLOR;
				};

				sampler2D _MainTex;
				sampler2D _DissolveTex;
				float4 _MainTex_ST;
				half _Value;
				half _DissSize;
				half4 _DissColor, _AddColor;

				v2f vert(appdata v)
				{
					v2f o;
					o.uv1 = v.texcoord1;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
					o.vertexColor = v.vertexColor;
					o.uv.zw = v.uv.zw;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.uv.xy);
					float dissolveValue = tex2D(_DissolveTex, i.uv.xy).r;
#if CUSTOMDATA_OPEN_YES
					float clipValue = dissolveValue - saturate(i.uv.z);
#else
					float clipValue = dissolveValue - _Value;
#endif
				
					clip(clipValue);
					clipValue = max(0, clipValue);
					if (clipValue < _DissSize)
					{
						half4 dissolveColor = lerp(_DissColor, _AddColor, clipValue / _DissSize) * 2;
						col *= dissolveColor;
					}
					col.rbg = col.rbg * i.vertexColor.rgb;
					col.a = i.vertexColor.a;
					return col;
				}
				ENDCG
			}
		}
}